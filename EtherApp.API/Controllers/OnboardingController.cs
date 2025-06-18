using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.Shared.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class OnboardingController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly IInterestService _interestService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFilesService _filesService;
        
        public OnboardingController(
            UserManager<User> userManager,
            IInterestService interestService,
            IWebHostEnvironment webHostEnvironment,
            IFilesService filesService)
        {
            _userManager = userManager;
            _interestService = interestService;
            _webHostEnvironment = webHostEnvironment;
            _filesService = filesService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            return Ok(ApiResponse<object>.SuccessResponse(new {
                user.UserName,
                user.FullName,
                user.ProfilePictureUrl
            }));
        }

        [HttpPost("profile")]
        public async Task<IActionResult> SetupProfile([FromForm] SetupProfileViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid model"));
            }
            
            // Check if username is already taken
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Username is already taken"));
            }
            
            user.UserName = model.UserName;
            
            // Handle profile picture upload if provided
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                user.ProfilePictureUrl = await _filesService.UploadImageAsync(model.ProfilePicture, ImageFileType.ProfileImage);
            }
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(string.Join(", ", result.Errors.Select(e => e.Description))));
            }
            
            return Ok(ApiResponse<object>.SuccessResponse(new {
                user.Id,
                user.UserName,
                user.ProfilePictureUrl
            }));
        }

        [HttpGet("interests")]
        public async Task<IActionResult> GetInterests()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            // Check if user already has interests
            var userInterests = await _interestService.GetUserInterestsAsync(userId.Value);
            var allInterests = await _interestService.GetAllInterestsAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new {
                AllInterests = allInterests,
                SelectedInterestIds = userInterests.Select(i => i.Id).ToList(),
                HasInterests = userInterests.Any()
            }));
        }

        [HttpPost("interests")]
        public async Task<IActionResult> SetInterests([FromBody] EditInterestsViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            if (model.SelectedInterestIds == null || model.SelectedInterestIds.Count == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("You must select at least one interest"));
            }

            await _interestService.UpdateUserInterestsAsync(userId.Value, model.SelectedInterestIds);

            return Ok(ApiResponse<object>.SuccessResponse("Interests updated successfully"));
        }
    }
}