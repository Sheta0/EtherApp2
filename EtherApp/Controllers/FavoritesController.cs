using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class FavoritesController : Controller
    {
        private readonly IPostsService _postsService;

        public FavoritesController(IPostsService postsService)
        {
            _postsService = postsService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myFavoritePosts = await _postsService.GetAllFavoritedPostsAsync(int.Parse(loggedInUser));

            return View(myFavoritePosts);
        }
    }
}
