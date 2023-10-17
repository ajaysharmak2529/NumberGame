using Microsoft.AspNetCore.SignalR;
using NumGameWeb.Data;
using System.Drawing;

namespace NumGameWeb
{
    public class BackgroundJobs : IHostedService {
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly IServiceScopeFactory _dependencyInjectionContainer;
        private readonly ILogger<BackgroundJobs> _logger;
        public List<BettingInfo>? data { get; set; }
        public List<CoinBetResponse>? CoinData { get; set; }
        private System.Timers.Timer timer;
        private double _interval = 2;
        private TimeSpan InitialTime;
        private Random rnd = new Random();


        public BackgroundJobs(ILogger<BackgroundJobs> logger, IHubContext<UpdateHub> hubContext, IServiceScopeFactory dependencyInjectionContainer) {
            _logger = logger;
            _hubContext = hubContext;
            _dependencyInjectionContainer = dependencyInjectionContainer;
            InitialTime = TimeSpan.FromMinutes(_interval);

        }
        public Task StartAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Background Service is starting.");
            // Set up and start your background job here            
            timer = new System.Timers.Timer(1000); // 1000 milliseconds = 1 second
            timer.Elapsed += SendTimeData;
            timer.Start();
            return Task.CompletedTask;
        }

        private void SendTimeData(object? sender, System.Timers.ElapsedEventArgs e)
        {
            InitialTime = InitialTime.Subtract(TimeSpan.FromSeconds(1));
            if (InitialTime.TotalSeconds <= 0) {
                InitialTime = TimeSpan.FromMinutes(_interval);
                _hubContext.Clients?.All.SendAsync("EnableButtons");
            }
            else {
                _hubContext.Clients?.All.SendAsync("UpdateTimer", $"<span>{InitialTime.Minutes}</span><span>{InitialTime.Seconds}</span>");
                if (InitialTime.TotalSeconds <= 10)
                    _hubContext.Clients?.All.SendAsync("DisableButtons");

                if (InitialTime.TotalSeconds <= 9) {
                    using (var scoped = _dependencyInjectionContainer.CreateScope()) {
                        var _service = scoped.ServiceProvider.GetRequiredService<IServices>();
                        var from_date = DateTime.Now.Subtract(TimeSpan.FromMinutes(_interval)).ToString("yyyy-MM-dd HH:mm:ss");
                        var to_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        data = _service.GetAllBettingDetails(from_date, to_date).GetAwaiter().GetResult();
                        CoinData = _service.GetAllInstallCoinBet(from_date, to_date).GetAwaiter().GetResult();
                    }
                }
                if (InitialTime.TotalSeconds <= 1) {
                    using (var scoped = _dependencyInjectionContainer.CreateScope()) {
                        var _service = scoped.ServiceProvider.GetRequiredService<IServices>();
                        CalculateNumberBettingResult(_service).GetAwaiter();
                        CalculateCoinBetResult(_service).GetAwaiter();
                    }
                }
            }
        }



        public async Task CalculateNumberBettingResult(IServices _service) {
            if (data != null && data.Count > 0) {
                var missingNumber = _service.GetMissingBettingNumbers(data);
                if (missingNumber != null && missingNumber.Count > 0) {
                    var index = rnd.Next(0, missingNumber.Count);
                    var openingNumber = missingNumber[index];
                    await _service.SaveOpenNumber(openingNumber);
                    var numbers = _service.GetRecentOpenNumbers().Result.data;
                    if (numbers != null && numbers.Count > 0) {
                        _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.Select(x => x.open_number).ToList()!);
                    }
                }
                else {
                    var minimumNumber = _service.GetNumberWithLowestAmount(data);
                    await _service.SaveOpenNumber(minimumNumber);
                    var numbers = _service.GetRecentOpenNumbers().Result.data;
                    if (numbers != null && numbers.Count > 0) {
                        _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.Select(x => x.open_number).ToList()!);
                    }
                    var userList = _service.GetUsersWhoBetOnLowestAmountNumber(data, minimumNumber);

                    foreach (var item in userList) {
                        var winningAmount = item.amount * 90;
                        var UpdatedWallet = await _service.UpdateUserWallet(item.user_id, winningAmount, item.number, item.user_token!);
                        var sendObj = new { WinningAmount = winningAmount, WinningNumber = item.number, wallet = UpdatedWallet };
                        _hubContext.Clients?.User(item.user_id.ToString()).SendAsync("NotifyWinner", sendObj);
                    }
                }
            }
            else {
                var openingNumber = rnd.Next(0, 99);
                await _service.SaveOpenNumber(openingNumber);
                var numbers = await _service.GetRecentOpenNumbers();
                if (numbers != null && numbers.data?.Count > 0) {
                    _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.data.Select(x => x.open_number).ToList()!);
                }
            }
        }
        public async Task CalculateCoinBetResult(IServices _service) {

            if (CoinData != null && CoinData.Count > 0) {
                var groupedBets = CoinData.GroupBy(b => b.coin_type);
                var installedSides = groupedBets.Select(g => g.Key).ToList();
                var missedSides = new List<string> { "head", "tail" }.Except(installedSides).ToList();
                if (missedSides.Any()) {
                    var side = missedSides.FirstOrDefault();
                    var res = _service.SaveOpenCoinSide(side?.ToLower()!);
                    _hubContext.Clients?.All.SendAsync("updateCoinResult", side!.ToUpperInvariant());
                }
                else
                {
                    var winningSide = groupedBets.OrderBy(g => g.Sum(b => b.amount)).FirstOrDefault()?.Key;
                    var result = CoinData.Where(x => x.coin_type == winningSide).ToList();
                    var res1 = _service.SaveOpenCoinSide(winningSide!.ToLower());
                    foreach (var item in result)
                    {
                        var winningAmount = item.amount * 2;
                        var walletResponse = await _service.UpdatedCoinWinnerWallet(item.user_token!, winningSide!, winningAmount);
                        var winningResultObj = new { amount = winningAmount, coinSide = winningSide, userWallte = walletResponse.wallet_balance };
                        _hubContext.Clients?.User(item.user_id.ToString()).SendAsync("NotifyCoinWinner", winningResultObj);
                    }
                    _hubContext.Clients?.All.SendAsync("updateCoinResult", winningSide!.ToUpperInvariant());
                }
            }
            else {
                 var res = rnd.Next(0, 10);
                 var coinSide = res < 5 ? "HEAD" : "TAIL";
                 var res1 = _service.SaveOpenCoinSide(coinSide!.ToLower());
                 _hubContext.Clients?.All.SendAsync("updateCoinResult", coinSide);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Background Service is stopping.");
            timer.Stop();
            return Task.CompletedTask;
        }
    }
}







