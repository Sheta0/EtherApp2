using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Stories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class StoriesController : BaseController
    {
        private readonly IStoriesService _storiesService;
        private readonly IFilesService _filesService;
        
        public StoriesController(IStoriesService storiesService, IFilesService filesService)
        {
            _storiesService = storiesService;
            _filesService = filesService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            // Get stories from user and their friends
            var userAndFriendStories = await _storiesService.GetUserAndFriendStoriesAsync(loggedInUser.Value);
            return View(userAndFriendStories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryVM storyVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            var imageUploadPath = await _filesService.UploadImageAsync(storyVM.Image, ImageFileType.StoryImage);

            var newStory = new Story
            {
                DateCreated = DateTime.Now,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = loggedInUser.Value
            };

            await _storiesService.CreateStoryAsync(newStory);

            return RedirectToAction("Index", "Home");
        }
    }
}
