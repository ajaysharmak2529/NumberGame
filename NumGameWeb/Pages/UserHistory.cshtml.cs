using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;
using System.Security.Claims;

namespace NumGameWeb.Pages {
    [Authorize]
    public class UserHistoryModel : PageModel {
        private readonly IServices _services;

        public UserHistoryModel(IServices services) {
            _services = services;
        }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 100;
        public string? PageType { get; set; }
        public List<UserHistoryObj>? UserNumberBettingHistory { get; set; }
        public List<CoinBetResponse>? UserCoinBettingHistory { get; set; }
        public async Task<IActionResult> OnGetAsync(string type, int? page) {
            PageType = type;
            if (type == "number") {
                CurrentPage = page ?? 1;
                var response = _services.GetUserHistory(new UserHistoryRequest { limit = PageSize, offset = (CurrentPage - 1) * PageSize, token = User.FindFirstValue("Token")! }).Result;
                UserNumberBettingHistory = response.data;
                TotalPages = (int)Math.Ceiling((double)response.row_count / PageSize);
            }else {
                CurrentPage = page ?? 1;
                var response = _services.GetUserCoinBetHistory(new UserHistoryRequest { limit = PageSize, offset = (CurrentPage - 1) * PageSize, token = User.FindFirstValue("Token")! }).Result;
                UserCoinBettingHistory = response.data;
                TotalPages = (int)Math.Ceiling((double)response.row_count / PageSize);
            }
            return Page();
        }        
    }
}
