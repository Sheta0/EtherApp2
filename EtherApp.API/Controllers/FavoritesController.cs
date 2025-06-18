using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class FavoritesController : BaseApiController
    {
        private readonly IPostsService _postsService;

        public FavoritesController(IPostsService postsService)
        {
            _postsService = postsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavoritePosts()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var favoritePosts = await _postsService.GetAllFavoritedPostsAsync(userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(favoritePosts));
        }
    }
}