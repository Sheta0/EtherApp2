using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.Shared.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class SettingsController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IFilesService _fileService;
        private readonly UserManager<User> _userManager;
        
        public SettingsController(
            IUserService userService, 
            IFilesService fileService, 
            UserManager<User> userManager)
        {
            _userService = userService;
            _fileService = fileService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            var userProfile = new {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.ProfilePictureUrl,
                user.Bio
            };

            return Ok(ApiResponse<object>.SuccessResponse(userProfile));
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureVM profilePictureVM)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            if (profilePictureVM.ProfilePicture == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("No image provided"));

            var uploadedProfilePictureUrl = await _fileService.UploadImageAsync(profilePictureVM.ProfilePicture, ImageFileType.ProfileImage);
            await _userService.UpdateUserProfilePicture(userId.Value, uploadedProfilePictureUrl);

            return Ok(ApiResponse<string>.SuccessResponse(uploadedProfilePictureUrl));
        }

        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileVM updateProfileVM)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            user.FullName = updateProfileVM.FullName;
            user.UserName = updateProfileVM.UserName;
            user.Bio = updateProfileVM.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new {
                user.Id,
                user.UserName,
                user.FullName,
                user.Bio
            }));
        }

        [HttpPost("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordVM updatePasswordVM)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            if (updatePasswordVM.NewPassword != updatePasswordVM.ConfirmPassword)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Passwords do not match"));
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, updatePasswordVM.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid current password"));
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordVM.CurrentPassword, updatePasswordVM.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            return Ok(ApiResponse<object>.SuccessResponse("Password updated successfully"));
        }
    }
}