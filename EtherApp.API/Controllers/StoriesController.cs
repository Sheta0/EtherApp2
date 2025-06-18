using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.Shared.ViewModels.Stories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class StoriesController : BaseApiController
    {
        private readonly IStoriesService _storiesService;
        private readonly IFilesService _filesService;
        
        public StoriesController(IStoriesService storiesService, IFilesService filesService)
        {
            _storiesService = storiesService;
            _filesService = filesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStories()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                
            var stories = await _storiesService.GetUserAndFriendStoriesAsync(userId.Value);
            return Ok(ApiResponse<List<Story>>.SuccessResponse(stories));
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromForm] StoryVM storyVM)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            if (storyVM.Image == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Image is required for a story"));

            var imageUploadPath = await _filesService.UploadImageAsync(storyVM.Image, ImageFileType.StoryImage);

            var newStory = new Story
            {
                DateCreated = DateTime.Now,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = userId.Value
            };

            var createdStory = await _storiesService.CreateStoryAsync(newStory);
            return Ok(ApiResponse<Story>.SuccessResponse(createdStory));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserStories(int userId)
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                
            var stories = await _storiesService.GetUserStoriesAsync(userId);
            return Ok(ApiResponse<List<Story>>.SuccessResponse(stories));
        }
    }
}