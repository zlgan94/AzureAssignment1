using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Grocery.WebApp.Data.Enums;
using Grocery.WebApp.Models;

namespace Grocery.WebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<MyIdentityUser> _userManager;
        private readonly SignInManager<MyIdentityUser> _signInManager;

        public IndexModel(
            UserManager<MyIdentityUser> userManager,
            SignInManager<MyIdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "How would you like your Name to be displayed?")]                                  // NOT NULL constraint for DB Schema
            [Display(Name = "Display Name")]            // constraint for Razor UI Label
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

        private async Task LoadAsync(MyIdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            // var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = user.PhoneNumber,     // phoneNumber,
                DisplayName = user.DisplayName,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }



            #region Original Code - Commented Block

            //if (!ModelState.IsValid)
            //{
            //    await LoadAsync(user);
            //    return Page();
            //}

            #endregion

            // this.User          Identity Object - ASP.NET Claims Principal
            // user               local variable of the MyIdentityUser (Identity data)
            // Input              InputViewModel

            // user.DisplayName   (value in the DB)         // State of the IdentityObject
            // Input.DisplayName  (textbox)                 // State in the UI



            if(!ModelState.IsValid)
            {
                return Page();              // errors on page. exit!
            }

            // -- Assign the values from the ViewModel to the User Object
            user.DisplayName = Input.DisplayName;
            user.DateOfBirth = Input.DateOfBirth;
            user.Gender = Input.Gender;

            // -- Update the Data into the Database using _userManager
            await _userManager.UpdateAsync(user);





            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
