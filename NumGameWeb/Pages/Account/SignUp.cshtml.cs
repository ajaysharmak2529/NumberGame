using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NumGameWeb.Data;
using System.ComponentModel.DataAnnotations;

namespace NumGameWeb.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly IServices _service;
        [BindProperty]
        public SignUpInput? Input { get; set; }


        public SignUpModel(ILogger<LoginModel> logger, IServices service)
        {
            _logger = logger;
            _service = service;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var AuthInput = new AuthModel { action = "signupForm", address = Input!.Address, email = Input.Email, full_name = Input.FullName, password = Input.Password, phone = Input.PhoneNumber };
                var res = await _service.RegisterUser(AuthInput);
                if (res.status == true)
                    return RedirectToAction("login", "Account");
                else
                {
                    ModelState.AddModelError("", res!.message!);
                    return Page();
                }                
            }
            else
                return Page();
            
        }
    }
    public class SignUpInput
    {
        [Required(ErrorMessage = "Please Enter Your Full Name!!")]
        [Display(Name ="Full Name")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone Number Must Have 10 digits.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone number must contain only numeric characters.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Your Email Address!!")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please Enter Password!!")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please Enter Confirm Password!!")]
        [Compare("Password", ErrorMessage = "Confirm Password Should Match With Password!!")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Please Enter Your Address!!")]
        public string? Address { get; set; }
    }
}
