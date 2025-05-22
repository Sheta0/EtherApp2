using EtherApp.Data;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.ViewComponents
{
    public class StoriesViewComponent : ViewComponent
    {
        private readonly IStoriesService _storiesService;
        public StoriesViewComponent(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var allStories = await _storiesService.GetAllStoriesAsync();

            return View(allStories);
        }
    }
}
