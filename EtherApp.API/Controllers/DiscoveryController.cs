using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class DiscoveryController : BaseApiController
    {
        private readonly IInterestService _interestService;
        private readonly IFriendsService _friendService;
        private readonly IPostsService _postsService;
        private readonly AppDBContext _context;
        
        public DiscoveryController(
            IInterestService interestService,
            IFriendsService friendService,
            IPostsService postsService,
            AppDBContext context)
        {
            _interestService = interestService;
            _friendService = friendService;
            _postsService = postsService;
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> DiscoverUsers(
            [FromQuery] bool filterByMyInterests = false, 
            [FromQuery] List<int> interests = null, 
            [FromQuery] int page = 1)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            // Get all available interests for filtering
            var allInterests = await _interestService.GetAllInterestsAsync();
            var pageSize = 12;

            // Find users based on filtering criteria
            List<(User User, double Similarity, List<Interest> SharedInterests)> discoveredUsers;

            if (filterByMyInterests)
            {
                discoveredUsers = await _interestService.GetSimilarUsersWithInterestsAsync(userId.Value, 100);
            }
            else if (interests != null && interests.Any())
            {
                discoveredUsers = await GetUsersByInterestsAsync(userId.Value, interests);
            }
            else
            {
                discoveredUsers = await _interestService.GetSimilarUsersWithInterestsAsync(userId.Value, 100);
            }

            // Get friend information
            var friends = await _friendService.GetUserFriendsAsync(userId.Value);
            var friendIds = friends
                .Select(f => f.SenderId == userId.Value ? f.ReceiverId : f.SenderId)
                .ToList();

            // Get pending requests
            var sentRequests = await _friendService.GetSentFriendRequestsAsync(userId.Value);
            var receivedRequests = await _friendService.GetReceivedFriendRequestsAsync(userId.Value);
            var pendingRequestUserIds = new List<int>();

            pendingRequestUserIds.AddRange(sentRequests.Select(r => r.ReceiverId));
            pendingRequestUserIds.AddRange(receivedRequests.Select(r => r.SenderId));

            // Apply pagination
            var totalItems = discoveredUsers.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var paginatedUsers = discoveredUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to response
            var userItems = new List<object>();
            foreach (var userInfo in paginatedUsers)
            {
                var user = userInfo.User;
                var similarity = userInfo.Similarity;
                var sharedInterests = userInfo.SharedInterests;

                var userPosts = await _postsService.GetUserPostsAsync(user.Id, userId.Value);
                var postContent = userPosts.FirstOrDefault()?.Content ?? string.Empty;
                var truncatedContent = !string.IsNullOrEmpty(postContent)
                    ? postContent.Substring(0, Math.Min(100, postContent.Length))
                    : string.Empty;

                userItems.Add(new {
                    User = new {
                        user.Id,
                        user.UserName,
                        user.FullName,
                        user.ProfilePictureUrl,
                        user.Bio
                    },
                    MatchPercentage = (int)similarity,
                    SharedInterests = sharedInterests,
                    IsFriend = friendIds.Contains(user.Id),
                    HasPendingFriendRequest = pendingRequestUserIds.Contains(user.Id),
                    MostRecentPost = truncatedContent,
                    PostCount = userPosts.Count
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(new {
                DiscoveredUsers = userItems,
                AvailableInterests = allInterests,
                SelectedInterests = interests ?? new List<int>(),
                FilterByMyInterests = filterByMyInterests,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            }));
        }

        private async Task<List<(User User, double Similarity, List<Interest> SharedInterests)>> GetUsersByInterestsAsync(int userId, List<int> interestIds)
        {
            // Find users who have the selected interests
            var usersWithInterests = await _context.UserInterests
                .Where(ui => interestIds.Contains(ui.InterestId))
                .Select(ui => ui.User)
                .Distinct()
                .Where(u => u.Id != userId)
                .ToListAsync();

            var result = new List<(User User, double Similarity, List<Interest> SharedInterests)>();

            foreach (var user in usersWithInterests)
            {
                var similarity = await _interestService.CalculateInterestSimilarityAsync(userId, user.Id);
                var sharedInterests = await _interestService.GetSharedInterestsAsync(userId, user.Id);
                result.Add((user, similarity, sharedInterests));
            }

            return result.OrderByDescending(u => u.Similarity).ToList();
        }
    }
}