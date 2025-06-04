using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SettingsController : ControllerBase
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Return user details without sensitive information
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.ProfilePictureUrl,
                user.Bio
            });
        }

        [HttpPut("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (dto.ProfilePicture == null)
                return BadRequest("No profile picture provided");

            var uploadedProfilePictureUrl = await _fileService.UploadImageAsync(
                dto.ProfilePicture, 
                ImageFileType.ProfileImage);

            await _userService.UpdateUserProfilePicture(user.Id, uploadedProfilePictureUrl);

            return Ok(new { profilePictureUrl = uploadedProfilePictureUrl });
        }

        [HttpPut("bio")]
        public async Task<IActionResult> UpdateBio([FromBody] UpdateBioDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            user.Bio = dto.Bio;
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true });
        }

        [HttpPut("name")]
        public async Task<IActionResult> UpdateFullName([FromBody] UpdateNameDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            user.FullName = dto.FullName;
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true });
        }
    }

    public class UpdateProfilePictureDto
    {
        public IFormFile ProfilePicture { get; set; } = null!;
    }

    public class UpdateBioDto
    {
        public string? Bio { get; set; }
    }

    public class UpdateNameDto
    {
        public string FullName { get; set; } = string.Empty;
    }
}