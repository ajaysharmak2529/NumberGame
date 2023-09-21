using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;

namespace NumGameWeb.Pages
{
    [Authorize]
    [BindProperties]
    public class CoinFlipModel : PageModel
    {
        public IServices _service { get; }


        public UserData? UserData { get; set; }
        public string CoinSide { get; set; } = string.Empty;
        public CoinFlipModel(IServices service)
        {
            _service = service;


        }
        
        public void OnGet()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var userData = _service.GetUserDetail().Result;
                if (userData != null)
                {
                    UserData = userData.data;
                    var res = _service.GetRecentOpenCoinBet().Result;
                    CoinSide = res.open_coin;

                }
                else
                {
                    UserData!.wallet = 0;
                }
            }
        }
    }
}
