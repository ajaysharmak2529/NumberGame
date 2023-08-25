using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Security.Claims;
using System.Linq;
using NumGameWeb.Data;
using System.Text.Json;

namespace NumGameWeb
{
    public class BackgroundJobs : IHostedService
    {
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly IServiceScopeFactory _dependencyInjectionContainer;
        private readonly ILogger<BackgroundJobs> _logger;
        private Timer _timer;
        private double _interval = 3;
        private int _executionCount = 0;
        private TimeSpan InitialTime;


        public BackgroundJobs(ILogger<BackgroundJobs> logger, IHubContext<UpdateHub> hubContext, IServiceScopeFactory dependencyInjectionContainer)
        {
            _logger = logger;
            _hubContext = hubContext;
            _dependencyInjectionContainer = dependencyInjectionContainer;
            InitialTime = TimeSpan.FromMinutes(_interval);

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Service is starting.");

            // Set up and start your background job here

            _timer = new Timer(DoWork, null, TimeSpan.Zero, InitialTime); // Run the job every _interval time minutes
            var timer = new System.Timers.Timer(1000); // 1000 milliseconds = 1 second
            timer.Elapsed += SendTimedata;
            timer.Start();
            return Task.CompletedTask;
        }

        private void SendTimedata(object? sender, System.Timers.ElapsedEventArgs e)
        {
            InitialTime = InitialTime.Subtract(TimeSpan.FromSeconds(1));
            if (InitialTime.TotalSeconds <= 0)
            {

                InitialTime = TimeSpan.FromMinutes(_interval);
                _hubContext.Clients?.All.SendAsync("EnableButtons");
            }
            else
            {
                _hubContext.Clients?.All.SendAsync("UpdateTimer", $"<span>{InitialTime.Minutes}</span><span>{InitialTime.Seconds}</span>");
                if (InitialTime.TotalSeconds <= 10)
                    _hubContext.Clients?.All.SendAsync("DisableButtons");
            }
        }

        private async void DoWork(object? state)
        {
            using (var scoped = _dependencyInjectionContainer.CreateScope())
            {
                var _service = scoped.ServiceProvider.GetRequiredService<IServices>();

                _logger.LogInformation("Background job is running at: {time}", DateTimeOffset.Now);
                int count = Interlocked.Increment(ref _executionCount);

                if (count > 1)
                {
                    var from_date = DateTime.Now.Subtract(TimeSpan.FromMinutes(_interval)).ToString("yyyy-MM-dd HH:mm:ss");
                    var to_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    var data = await _service.GetAllBettingDetails(from_date, to_date);
                    
                    if (data.Count>0)
                    {
                        var missingNumber = _service.GetMissingBettingNumbers(data);
                        if (missingNumber.Count > 0)
                        {

                            Random rnd = new Random();
                            var index = rnd.Next(0, missingNumber.Count);

                            var openningNumber = missingNumber[index];

                            // Save Opening number to database
                            await _service.SaveOpenNumber(openningNumber);

                            // Get saved Recent Opening Number
                            var numbers = _service.GetRecentOpenNumbers().Result.data;

                            // Updated to UI
                            if (numbers.Count > 0)
                            {
                                _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.Select(x => x.open_number).ToList()!);
                            }

                        }
                        else
                        {

                            var minmumNumber = _service.GetNumberWithLowestAmount(data);

                            await _service.SaveOpenNumber(minmumNumber);

                            // Get saved Recent Opening Number
                            var numbers = _service.GetRecentOpenNumbers().Result.data;

                            // Updated to UI
                            if (numbers.Count > 0)
                            {
                                _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.Select(x => x.open_number).ToList()!);
                            }

                            var userList = _service.GetUsersWhoBetOnLowestAmountNumber(data, minmumNumber);

                            foreach (var item in userList)
                            {
                                //calculate winning ammount of user
                                var winningAmount = item.amount * 90;


                                //Update User Balance
                                var UpdatedWallet = await _service.UpdateUserWallet(item.user_id, winningAmount, item.number, item.user_token!);

                                var sendObj = new { WinningAmount = winningAmount, WinningNumber = item.number, wallet = UpdatedWallet };

                                _hubContext.Clients?.User(item.user_id.ToString()).SendAsync("NotifyWinner", sendObj);
                            }                           
                        }

                    }
                    else
                    {
                        Random rnd = new Random();
                        var openningNumber = rnd.Next(0, 99);

                        // Save Opening number to database
                        await _service.SaveOpenNumber(openningNumber);

                        // Get saved Recent Opening Number
                        var numbers = await _service.GetRecentOpenNumbers();

                        var sendObj = new { WinningAmount = 1000, WinningNumber = openningNumber };


                        _hubContext.Clients?.User("4601").SendAsync("NotifyWinner", sendObj);
                        // Updated to UI
                        if (numbers.data?.Count > 0)
                        {
                            _hubContext.Clients?.All.SendAsync("updateOpenNumberAndList", numbers.data.Select(x => x.open_number).ToList()!);
                        }

                    }
                }



            _logger.LogInformation("{Service} is working, execution count: {Count:#,0}", nameof(NumGameWeb), count);
            }




        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Service is stopping.");

            // Stop the background job here
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
    public class OpeningNumber
    {
        public int open_number { get; set; }
        public string? open_date { get; set; }
    }

}
    



    


