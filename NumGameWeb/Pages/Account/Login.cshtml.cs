using NumGameWeb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore;
using System.Text.Json;

namespace NumGameWeb.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly IServices _service;

        public LoginModel(ILogger<LoginModel> logger, IServices service )
        {
            _logger = logger;
            _service = service;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public bool IsRememberMe { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name ="Email/Phone Number")]
            public string? Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.Response.Redirect("/index");
            }

            ReturnUrl = returnUrl;
        }
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                // Use Input.Email and Input.Password to authenticate the user
                // with your custom authentication logic.
                //
                // For demonstration purposes, the sample validates the user
                // on the email address maria.rodriguez@contoso.com with 
                // any password that passes model validation.

                var user = await AuthenticateUser(Input.Email!, Input.Password!);
                
                if (user.status == false)
                {
                    ModelState.AddModelError(string.Empty, user.message!);
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name !, user.data!.full_name!),
                    new Claim("FullName" !, user.data.email !),
                    new Claim("Token" !, user.data.token !),
                    new Claim(ClaimTypes.NameIdentifier, user.data.user_id.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = IsRememberMe,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), 
                    authProperties);

                var clms = User.FindFirstValue("Token");

                _logger.LogInformation("User {Email} logged in at {Time}.", 
                    user.data.email, DateTime.UtcNow);

                return LocalRedirect(Url.GetLocalUrl(returnUrl));
            }

            // Something failed. Redisplay the form.
            return Page();
        }
        
        public async Task<ResponseResult<ApplicationUser>> AuthenticateUser(string email, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://globalbigwin.com/api/auth/login");                    
                    var userobj = new { username = email, password = password };

                    var content = new StringContent(JsonSerializer.Serialize(userobj), null, "application/json");
                    request.Content = content;
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<ResponseResult<ApplicationUser>>(str);
                        return data;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return null;
            }
        }
    }
}
