using NumGameWeb.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace NumGameWeb
{
    public class Services : IServices
    {

        public IHttpContextAccessor _HttpContextAccessor { get; }
        public Services(IHttpContextAccessor httpContext)
        {
            _HttpContextAccessor = httpContext;

        }

        public async Task<BetSaveResponce> SaveBettingData(BettingSaveRequest betting)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/betting/insert");

                var content = new StringContent(JsonSerializer.Serialize(betting), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<BetSaveResponce>(await response.Content.ReadAsStringAsync())!;

                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null;
            }
        }





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

        public async Task<UserDetail> GetUserDetail()
        {
            try
            {
                var token = GetUserToken();

                if (!string.IsNullOrEmpty(token))
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/user/detail");
                    var reqobj = new { token = token };
                    var content = new StringContent(JsonSerializer.Serialize(reqobj), null, "application/json");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {

                        return JsonSerializer.Deserialize<UserDetail>(await response.Content.ReadAsStringAsync())!;
                    }
                    else
                    {
                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null!;
            }



        }



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

        public async Task<List<BettingInfo>> GetAllBettingDetails(string from, string to)
        {
            try
            {               
                    var requestObj = new { date_from = from, date_to = to };
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/betting/fetch");
                    var content = new StringContent(JsonSerializer.Serialize(requestObj), null, "application/json");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responce = JsonSerializer.Deserialize<ResponseResult<List<BettingInfo>>>(await response.Content.ReadAsStringAsync());
                    return responce.data!;                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }


        }
        public async Task SaveOpenNumber(int number)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/open-number/insert");
                var requestobj = new
                {
                    open_number = number.ToString(),
                    open_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm-ss")
                };
                var content = new StringContent(JsonSerializer.Serialize(requestobj), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        public async Task<ResponseResult<List<OpeningNumber>>> GetRecentOpenNumbers()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/open-number/fetch-recent-num");
                var requobj = new { limit = 10 };
                var content = new StringContent(JsonSerializer.Serialize(requobj), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<ResponseResult<List<OpeningNumber>>>(await response.Content.ReadAsStringAsync())!;
                    return data;
                }
                else
                {
                    return null!;
                }
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
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/user/update-user-wallet");
                var reqObj = new { user_id = userId, amount = amount, number = number , token = token};
                var content = new StringContent(JsonSerializer.Serialize(reqObj), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var resData = JsonSerializer.Deserialize<WalletResponce>(await response.Content.ReadAsStringAsync());
                    return resData.wallet_balance;

                }
                else
                {
                    return -1;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public Task<string> GetUserIdBySession()
        {
            HttpContext httpContext = _HttpContextAccessor.HttpContext!;
            if (httpContext != null)
            {
                return Task.FromResult<string>(httpContext.Session.GetString("userId")!);// _HttpContextAccessor.HttpContext.Session.GetString("userId")! //.Session?.GetString("userId");

            }
            else
            {
                return Task.FromResult(string.Empty);
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
    }

    public class BetSaveResponce
    {
        public bool status { get; set; }
        public string message { get; set; }
        public decimal wallet_balance { get; set; }
    }


}
