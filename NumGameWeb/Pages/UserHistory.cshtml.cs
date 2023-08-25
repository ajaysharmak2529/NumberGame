using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;
using System.Security.Claims;
using System.Text.Json;

namespace NumGameWeb.Pages
{
    [Authorize]
    public class UserHistoryModel : PageModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 100;
        public List<UserHistoryObj>? UserHistories { get; set; }
        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "page")] int? page)
        {            
                CurrentPage = page ?? 1;
                var responce =  GetUserHistory(new UserHistoryRequest { limit = PageSize, offset = (CurrentPage - 1) * PageSize, token = User.FindFirstValue("Token")! }).Result;
                UserHistories = responce.data;
                TotalPages = (int)Math.Ceiling((double)responce.row_count / PageSize);
            
            return Page();

        }

        public async Task<UserHistoryResponce> GetUserHistory(UserHistoryRequest userHistoryRequest)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/user/user-payment-history");
                var content = new StringContent(JsonSerializer.Serialize(userHistoryRequest), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<UserHistoryResponce>(await response.Content.ReadAsStringAsync())!;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
