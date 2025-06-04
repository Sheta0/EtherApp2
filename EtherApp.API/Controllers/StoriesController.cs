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
    public class StoriesController : ControllerBase
    {
        private readonly IStoriesService _storiesService;
        private readonly IFilesService _filesService;
        private readonly UserManager<User> _userManager;

        public StoriesController(
            IStoriesService storiesService,
            IFilesService filesService,
            UserManager<User> userManager)
        {
            _storiesService = storiesService;
            _filesService = filesService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStories()
        {
            var stories = await _storiesService.GetAllStoriesAsync();
            return Ok(stories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromForm] CreateStoryDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (dto.Image == null)
                return BadRequest("Story must include an image");

            string imageUrl = await _filesService.UploadImageAsync(dto.Image, ImageFileType.StoryImage);

            var story = new Story
            {
                UserId = user.Id,
                ImageUrl = imageUrl,
                DateCreated = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdStory = await _storiesService.CreateStoryAsync(story);
            return Ok(createdStory);
        }
    }

    public class CreateStoryDto
    {
        public IFormFile? Image { get; set; }
    }
}