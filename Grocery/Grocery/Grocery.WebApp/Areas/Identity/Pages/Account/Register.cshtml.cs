using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Grocery.WebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Grocery.WebApp.Data.Enums;

namespace Grocery.WebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<MyIdentityUser> _signInManager;
        private readonly UserManager<MyIdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<MyIdentityUser> userManager,
            SignInManager<MyIdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        // The ViewModel for the Register Page
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "How would you like your Name to be displayed?")]                                  // NOT NULL constraint for DB Schema
            [Display(Name = "Display Name")]             // constraint for Razor UI Label
            [MinLength(2)]                              // constraint for UI
            [MaxLength(60)]                             // constraint for UI
            public string DisplayName { get; set; }

            [Required(ErrorMessage = "Please indicate which of these best describes your Gender")]
            [Display(Name = "Gender")]
            public MyAppGenderTypes Gender { get; set; }

            [Required(ErrorMessage = "Your Date of Birth please.")]
            [Display(Name = "Date of Birth")]
            public DateTime DateOfBirth { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new MyIdentityUser 
                { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    DateOfBirth = Input.DateOfBirth,
                    DisplayName = Input.DisplayName,
                    Gender = Input.Gender,
                    IsAdminUser = false             // Customer User, hence false.
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add the user to the Role (Customer)
                    await _userManager.AddToRoleAsync(user, MyAppRoleTypes.Customer.ToString());


                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        try
                        {
                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        }
                        catch(Grocery.WebApp.Services.MyEmailSenderException exp)
                        {
                            string message
                                = $"User Created!! Confirm using {callbackUrl}. Error: {exp.Message}";
                            ModelState.AddModelError("RegistrationEmail", message);
                            return Page();
                        }

                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
