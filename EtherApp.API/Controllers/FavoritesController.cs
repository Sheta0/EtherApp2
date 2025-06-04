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
    public class FavoritesController : ControllerBase
    {
        private readonly IPostsService _postsService;
        private readonly UserManager<User> _userManager;

        public FavoritesController(
            IPostsService postsService, 
            UserManager<User> userManager)
        {
            _postsService = postsService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavoritePosts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var favoritePosts = await _postsService.GetAllFavoritedPostsAsync(user.Id);
            return Ok(favoritePosts);
        }
    }
}