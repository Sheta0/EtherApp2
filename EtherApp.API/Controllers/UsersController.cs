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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFilesService _filesService;
        private readonly UserManager<User> _userManager;

        public UsersController(
            IUserService userService,
            IFilesService filesService,
            UserManager<User> userManager)
        {
            _userService = userService;
            _filesService = filesService;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetUserPosts(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var posts = await _userService.GetUserPosts(id, currentUser.Id);
            return Ok(posts);
        }

        [HttpPut("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            string imageUrl = null;
            if (dto.Image != null)
            {
                imageUrl = await _filesService.UploadImageAsync(dto.Image, ImageFileType.ProfileImage);
            }

            await _userService.UpdateUserProfilePicture(user.Id, imageUrl);
            return Ok(new { ProfilePictureUrl = imageUrl });
        }
    }

    public class UpdateProfilePictureDto
    {
        public IFormFile? Image { get; set; }
    }
}