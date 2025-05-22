using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IFilesService _fileService;
        private readonly UserManager<User> _userManager;

        public SettingsController(IUserService userService, IFilesService fileService, UserManager<User> userManager)
        {
            _userService = userService;
            _fileService = fileService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            return View(loggedInUser);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(UpdateProfilePictureVM profilePictureVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            var uploadedProfilePictureUrl = await _fileService.UploadImageAsync(profilePictureVM.ProfilePicture, ImageFileType.ProfileImage);

            await _userService.UpdateUserProfilePicture(loggedInUser.Value, uploadedProfilePictureUrl);

            return RedirectToAction("Index");
        }



    }
}
