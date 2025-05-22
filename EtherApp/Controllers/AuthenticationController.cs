using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.ViewModels.Authentication;
using EtherApp.ViewModels.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        #region Login
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var existingUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (existingUser is null)
            {
                ModelState.AddModelError("Email", "Invalid Email or Password");
                return View(loginVM);
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(existingUser);
            if (!existingUserClaims.Any(C => C.Type == CustomClaim.FullName))
            {
                await _userManager.AddClaimAsync(existingUser, new Claim(CustomClaim.FullName, existingUser.FullName));
            }

            var result = await _signInManager.PasswordSignInAsync(existingUser.UserName, loginVM.Password, loginVM.RememberMe, false);

            if (result.Succeeded)
            {
                if(User.IsInRole(AppRoles.Admin))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }


            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(loginVM);
        }
        #endregion

        #region Register
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var user = new User
            {
                UserName = registerVM.Email,
                Email = registerVM.Email,
                FullName = $"{registerVM.FirstName} {registerVM.LastName}"
            };

            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
            if (existingUser is not null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(registerVM);
            }

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, AppRoles.User);
                await _userManager.AddClaimAsync(user, new Claim(CustomClaim.FullName, user.FullName));
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Welcome", "Onboarding");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            return View(registerVM);
        }
        #endregion

        #region SignOut
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordVM updatePasswordVM)
        {
            if (updatePasswordVM.NewPassword != updatePasswordVM.ConfirmPassword)
            {
                TempData["PasswordError"] = "Passwords do not match";
                TempData["ActiveTab"] = "Password";

                return RedirectToAction("Index", "Settings");
            }

            var loggedInUserId = await _userManager.GetUserAsync(User);
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(loggedInUserId, updatePasswordVM.CurrentPassword);

            if (!isCurrentPasswordValid)
            {
                TempData["PasswordError"] = "Invalid Current Password";
                TempData["ActiveTab"] = "Password";
                return RedirectToAction("Index", "Settings");
            }

            var result = await _userManager.ChangePasswordAsync(loggedInUserId, updatePasswordVM.CurrentPassword, updatePasswordVM.NewPassword);

            if (result.Succeeded)
            {
                TempData["PasswordSuccess"] = "Password Updated Successfully";
                TempData["ActiveTab"] = "Password";

                await _signInManager.RefreshSignInAsync(loggedInUserId);
            }

            return RedirectToAction("Index", "Settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileVM updateProfileVM)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser is null)
                return RedirectToAction("Login");

            loggedInUser.FullName = updateProfileVM.FullName;
            loggedInUser.UserName = updateProfileVM.UserName;
            loggedInUser.Bio = updateProfileVM.Bio;

            var result = await _userManager.UpdateAsync(loggedInUser);
            if (result.Succeeded)
            {
                TempData["ProfileSuccess"] = "Profile Updated Successfully";
                TempData["ActiveTab"] = "Profile";
            }
            else
                TempData["ProfileError"] = "Profile Update Failed";

            return RedirectToAction("Index", "Settings");
        }


        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Authentication");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (info == null)
                return RedirectToAction("Login");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new User()
                {
                    UserName = email,
                    Email = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(newUser);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, AppRoles.User);
                    await _userManager.AddClaimAsync(newUser, new Claim(CustomClaim.FullName, newUser.FullName));
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
    }
}
