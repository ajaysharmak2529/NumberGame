using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;

namespace NumGameWeb.Pages
{
    [Authorize]
    public class CoinFlipModel : PageModel
    {
        public IServices _service { get; }

        [BindProperty]
        public UserData? UserData { get; set; }
        public CoinFlipModel(IServices service)
        {
            _service = service;
        }
        public void OnGet()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var userdata = _service.GetUserDetail().Result;
                if (userdata != null)
                {
                    UserData = userdata.data;
                    var userid = _service.GetUserID();
                }
                else
                {
                    UserData.wallet =0;
                }




            }
        }
    }
}
