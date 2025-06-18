using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class RecommendationsController : BaseApiController
    {
        private readonly IPostsService _postsService;
        private readonly IInterestService _interestService;
        
        public RecommendationsController(IPostsService postsService, IInterestService interestService)
        {
            _postsService = postsService;
            _interestService = interestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations([FromQuery] bool filterByMyInterests = false, [FromQuery] List<int> interests = null)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                
            // Get all available interests for filtering
            var allInterests = await _interestService.GetAllInterestsAsync();
            
            // Get posts based on filters
            List<Post> recommendedPosts;
            
            if (filterByMyInterests)
            {
                var userInterests = await _interestService.GetUserInterestsAsync(userId.Value);
                var userInterestIds = userInterests.Select(i => i.Id).ToList();
                recommendedPosts = await GetPostsByInterestIdsAsync(userId.Value, userInterestIds);
            }
            else if (interests != null && interests.Any())
            {
                recommendedPosts = await GetPostsByInterestIdsAsync(userId.Value, interests);
            }
            else
            {
                recommendedPosts = await _postsService.GetRecommendedPostsAsync(userId.Value);
            }
            
            // Get similar users
            var similarUsers = await _interestService.GetSimilarUsersWithInterestsAsync(userId.Value);
            var mappedSimilarUsers = similarUsers.Select(x => new {
                User = new {
                    x.User.Id,
                    x.User.UserName,
                    x.User.FullName,
                    x.User.ProfilePictureUrl,
                    x.User.Bio
                },
                Similarity = (int)x.Similarity,
                SharedInterests = x.SharedInterests
            }).ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(new {
                RecommendedPosts = recommendedPosts,
                SimilarUsers = mappedSimilarUsers,
                AvailableInterests = allInterests,
                SelectedInterests = interests ?? new List<int>(),
                FilterByMyInterests = filterByMyInterests
            }));
        }
        
        private async Task<List<Post>> GetPostsByInterestIdsAsync(int userId, List<int> interestIds)
        {
            if (interestIds == null || !interestIds.Any())
                return new List<Post>();
                
            var posts = await _postsService.GetAllPostsAsync(userId);
            
            return posts
                .Where(p => !p.IsPrivate && p.UserId != userId)
                .Where(p => p.Interests != null && p.Interests.Any(i => interestIds.Contains(i.InterestId)))
                .OrderByDescending(p => p.Interests.Count(i => interestIds.Contains(i.InterestId)))
                .ThenByDescending(p => p.DateCreated)
                .ToList();
        }
    }
}