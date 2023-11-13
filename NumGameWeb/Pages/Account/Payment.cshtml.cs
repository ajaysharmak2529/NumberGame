using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NumGameWeb.Pages.Account
{
    public class PaymentModel : PageModel
    {
        [BindProperty]
        public string? Url { get; set; }
        public void OnGet([FromQuery]string type)
        {
            if (type != null) 
            {                
                if (type.Equals("add")) 
                {
                    Url = "https://globalbigwin.com/money/add";
                }
                if (type.Equals("debit")) 
                {
                    Url = "https://globalbigwin.com/payout/fund-account";
                }

            }
        }
    }
}
