using NumGameWeb.Data;
using System.Security.Claims;
using System.Text.Json;

namespace NumGameWeb
{
    public class Services : IServices
    {
        public IHttpContextAccessor _HttpContextAccessor { get; }

        private HttpClient _httpClient = new HttpClient();
        private string base_url = "https://globalbigwin.com";
        public Services(IHttpContextAccessor httpContext)
        {
            _HttpContextAccessor = httpContext;
        }


        #region Flip Coin Methods
        public async Task<BetSaveResponce> SaveCoinBet(CoinBetSaveRequest bet)
        {
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/betting/insert", body: JsonSerializer.Serialize(bet));
            if (response != null && response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<BetSaveResponce>(await response.Content.ReadAsStringAsync())!;
            else return null!;
        }

        public async Task<bool> SaveOpenCoinSide(string CoinSide)
        {
            var requestBody = new SaveOpenCoin() { open_coin = CoinSide, open_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/open-number/insert", body: JsonSerializer.Serialize(requestBody));
            if (response != null && response.IsSuccessStatusCode) return true;
            else return false;
        }

        public async Task<List<CoinBetResponse>> GetAllInstallCoinBet(string from, string to)
        {
            var requestBody = new { date_from = from, date_to = to };
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/betting/fetch", body: JsonSerializer.Serialize(requestBody));
            if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<CoinBetResponse>>>(await response.Content.ReadAsStringAsync())!.data!;
            else return null!;
        }

        public async Task<WalletResponce> UpdatedCoinWinnerWallet(string token, string coinSide, decimal amount)
        {
            var requestBody = new { token = token, coin_type = coinSide, amount = amount };
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/user/update-user-wallet", body: JsonSerializer.Serialize(requestBody));
            if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<WalletResponce>(await response.Content.ReadAsStringAsync())!;
            else return null!;
        }

        public async Task<ResponseResult<List<CoinBetResponse>>> GetUserCoinHistory(UserHistoryRequest requestBody)
        {
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/betting/all-user-betting-detail", body: JsonSerializer.Serialize(requestBody));
            if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<CoinBetResponse>>>(await response.Content.ReadAsStringAsync())!;
            else return null!;
        }

        public async Task<RecentOpenedCoin> GetRecentOpenCoinBet()
        {
            var requestBody = new { limit = 1 };
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/open-number/fetch-recent-num", body: JsonSerializer.Serialize(requestBody));
            if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<RecentOpenedCoin>>>(await response.Content.ReadAsStringAsync())!.data!.FirstOrDefault();
            else return null!;
        }
        public async Task<ResponseResult<List<CoinBetResponse>>> GetUserCoinBetHistory(UserHistoryRequest userHistoryRequest)
        {
            var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/coin-game/user/user-payment-history", body: JsonSerializer.Serialize(userHistoryRequest));
            if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<CoinBetResponse>>>(await response.Content.ReadAsStringAsync())!;
            else return null!;
        }

        #endregion

        #region Common Methods
        public string GetUserToken()
        {
            if (_HttpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                return _HttpContextAccessor.HttpContext!.User.FindFirstValue("Token")!;
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task<string> GetUserID()
        {
            HttpContext httpContext = _HttpContextAccessor.HttpContext!;
            if (httpContext!.User!.Identity!.IsAuthenticated)
            {
                return _HttpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task<UserDetail> GetUserDetail()
        {
            try
            {
                var token = GetUserToken();

                if (!string.IsNullOrEmpty(token))
                {
                    var reqobj = new { token = token };
                    var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/user/detail", body: JsonSerializer.Serialize(reqobj));
                    if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<UserDetail>(await response.Content.ReadAsStringAsync())!;
                    else return null!;
                }
                else return null!;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null!;
            }
        }
        #endregion
        public List<int> GetMissingBettingNumbers(List<BettingInfo> userBets)
        {
            List<int> allNumbers = Enumerable.Range(0, 100).ToList();
            List<int> betNumbers = userBets.Select(bet => bet.number).Distinct().ToList();
            List<int> missingNumbers = allNumbers.Except(betNumbers).ToList();
            return missingNumbers;
        }
        public int GetNumberWithLowestAmount(List<BettingInfo> userBets)
        {
            var numberGroups = userBets.GroupBy(bet => bet.number);
            var minAmountNumber = numberGroups.OrderBy(group => group.Sum(bet => bet.amount)).FirstOrDefault();
            return minAmountNumber?.Key ?? -1; // Return -1 if no bets are placed yet.
        }
        public List<BettingInfo> GetUsersWhoBetOnLowestAmountNumber(List<BettingInfo> userBets, int winNumber)
        {
            if (userBets.Count > 0)
            {
                var betsOnMinAmountNumber = userBets.Where(x => x.number == winNumber).ToList();
                return betsOnMinAmountNumber;
            }
            else
            {
                return new List<BettingInfo>(); // Return an empty list if no bets are placed yet.
            }
        }

        #region Number betting Methods
        public async Task<BetSaveResponce> SaveBettingData(BettingSaveRequest betting)
        {
            try
            {
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/betting/insert", body: JsonSerializer.Serialize(betting));
                if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<BetSaveResponce>(await response.Content.ReadAsStringAsync())!;
                else return null!;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null;
            }
        }
        public async Task<UserHistoryResponse> GetUserHistory(UserHistoryRequest userHistoryRequest)
        {
            try
            {
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/user/user-payment-history", body: JsonSerializer.Serialize(userHistoryRequest));
                if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<UserHistoryResponse>(await response.Content.ReadAsStringAsync())!;
                else return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<BettingInfo>> GetAllBettingDetails(string from, string to)
        {
            try
            {
                var requestObj = new { date_from = from, date_to = to };
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/betting/fetch", body: JsonSerializer.Serialize(requestObj));
                if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<BettingInfo>>>(await response.Content.ReadAsStringAsync())!.data!;
                else return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<bool> SaveOpenNumber(int number)
        {
            try
            {
                var requestObj = new
                {
                    open_number = number.ToString(),
                    open_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm-ss")
                };
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/open-number/insert", body: JsonSerializer.Serialize(requestObj));
                if (response != null && response.IsSuccessStatusCode) return true;
                else return false;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return false;
            }
        }
        public async Task<ResponseResult<List<OpeningNumber>>> GetRecentOpenNumbers()
        {
            try
            {
                var requestObj = new { limit = 10 };
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/open-number/fetch-recent-num", body: JsonSerializer.Serialize(requestObj));
                if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<ResponseResult<List<OpeningNumber>>>(await response.Content.ReadAsStringAsync())!;
                else return null!;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null!;
            }
        }
        public async Task<decimal> UpdateUserWallet(int userId, int amount, int number, string token)
        {
            try
            {
                var reqObj = new { user_id = userId, amount = amount, number = number, token = token };
                var response = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/user/update-user-wallet", body: JsonSerializer.Serialize(reqObj));
                if (response != null && response.IsSuccessStatusCode) return JsonSerializer.Deserialize<WalletResponce>(await response.Content.ReadAsStringAsync())!.wallet_balance;
                else return -1!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public async Task<ResponseResult<ApplicationUser>> RegisterUser(AuthModel authModel)
        {
            var result = await RequestHelper.SendHttpRequest(_httpClient, HttpMethod.Post, $"{base_url}/api/auth/login", body: JsonSerializer.Serialize(authModel));
            var res = await result!.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ResponseResult<ApplicationUser>>(res);
            return data!;
        }
        #endregion
    }

    public class RecentOpenedCoin
    {
        public string? open_coin { get; set; }
        public string? open_date { get; set; }
    }


}
