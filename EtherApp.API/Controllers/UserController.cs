using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly IInterestService _interestService;
        private readonly IPostsService _postService;

        public UserController(
            UserManager<User> userManager,
            IInterestService interestService,
            IPostsService postService)
        {
            _userManager = userManager;
            _interestService = interestService;
            _postService = postService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            var profileUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (profileUser == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            // Calculate similarity percentage
            double similarityPercentage = await _interestService.CalculateInterestSimilarityAsync(
                currentUserId.Value, profileUser.Id);

            // Get user posts
            var userPosts = await _postService.GetUserPostsAsync(userId, currentUserId.Value);

            // Get user interests
            var userInterests = await _interestService.GetUserInterestsAsync(userId);

            var result = new
            {
                User = new
                {
                    profileUser.Id,
                    profileUser.UserName,
                    profileUser.FullName,
                    profileUser.Email,
                    profileUser.ProfilePictureUrl,
                    profileUser.Bio
                },
                Posts = userPosts,
                Interests = userInterests,
                SimilarityPercentage = similarityPercentage,
                IsCurrentUser = currentUserId.Value == userId
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpGet("interests")]
        public async Task<IActionResult> GetUserInterests()
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            var allInterests = await _interestService.GetAllInterestsAsync();
            var userInterests = await _interestService.GetUserInterestsAsync(currentUserId.Value);

            var result = new
            {
                AllInterests = allInterests,
                SelectedInterestIds = userInterests.Select(i => i.Id).ToList()
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpPost("interests")]
        public async Task<IActionResult> UpdateUserInterests([FromBody] EditInterestsViewModel model)
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            // Check if any interests are selected
            if (model.SelectedInterestIds == null || model.SelectedInterestIds.Count == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("You must select at least one interest"));
            }

            await _interestService.UpdateUserInterestsAsync(currentUserId.Value, model.SelectedInterestIds);

            return Ok(ApiResponse<object>.SuccessResponse("Interests updated successfully"));
        }

        [HttpGet("related-posts")]
        public async Task<IActionResult> GetUserRelatedPosts([FromQuery] int userId, [FromQuery] int currentPostId)
        {
            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            // Get user posts
            var userPosts = await _postService.GetUserPostsAsync(userId, currentUserId.Value);
            
            // Filter out the current post and take only a few recent ones
            var relatedPosts = userPosts
                .Where(p => p.Id != currentPostId)
                .OrderByDescending(p => p.DateCreated)
                .Take(3)
                .Select(p => new
                {
                    p.Id,
                    Title = !string.IsNullOrEmpty(p.Content) 
                        ? (p.Content.Length > 50 ? p.Content.Substring(0, 50) + "..." : p.Content) 
                        : "Post without text",
                    p.DateCreated,
                    DaysAgo = (DateTime.Now - p.DateCreated).Days,
                    HasImage = !string.IsNullOrEmpty(p.ImageUrl)
                })
                .ToList();

            return Ok(ApiResponse<object>.SuccessResponse(relatedPosts));
        }

        [HttpGet("stats/{userId}")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            var currentUserId = GetUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            // Get post count
            var userPosts = await _postService.GetUserPostsAsync(userId, currentUserId.Value);
            var postCount = userPosts?.Count ?? 0;
            
            // Get friendship information
            var friendsService = HttpContext.RequestServices.GetService<IFriendsService>();
            int friendsCount = 0;
            
            if (friendsService != null)
            {
                var userFriends = await friendsService.GetUserFriendsAsync(userId);
                friendsCount = userFriends.Count;
            }

            return Ok(ApiResponse<object>.SuccessResponse(new { 
                postCount,
                friendsCount
            }));
        }
    }
}