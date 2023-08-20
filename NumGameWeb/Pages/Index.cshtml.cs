using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;

namespace NumGameWeb.Pages
{
    [Authorize]
    public class indexModel : PageModel
    {
        public List<int>? PreviosOpnings { get; set; } 
        [BindProperty]
        public UserData? UserData { get; set; }
        [BindProperty]
        public int OpenNumber { get; set; }
        public IServices _service { get; }

        public indexModel(IServices service)
        {
            _service = service;
        }
        public async void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
               var userdata = _service.GetUserDetail().Result;
                UserData = userdata.data;
                //_service.GetUserIdBySession();
                var userid = _service.GetUserID();
                var data = _service.GetRecentOpenNumbers().Result.data;


                if (data != null)
                {
                    PreviosOpnings = data.Select(x => x.open_number).ToList();
                    OpenNumber = PreviosOpnings.FirstOrDefault();
                }
                else
                {
                    PreviosOpnings = new List<int>();
                }
            }
            
        }


    }
}
