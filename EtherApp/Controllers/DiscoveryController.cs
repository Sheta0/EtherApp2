using EtherApp.Controllers.Base;
using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Discovery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtherApp.Controllers
{
    public class DiscoveryController : BaseController
    {
        private readonly IInterestService _interestService;
        private readonly IFriendsService _friendService;
        private readonly IPostsService _postsService;

        public DiscoveryController(
            IInterestService interestService,
            IFriendsService friendService,
            IPostsService postsService)
        {
            _interestService = interestService;
            _friendService = friendService;
            _postsService = postsService;
        }

        public async Task<IActionResult> Users(bool filterByMyInterests = false, List<int> interests = null, int page = 1)
        {
            var userId = GetUserId() ?? 0;
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Get all available interests for filtering
            var allInterests = await _interestService.GetAllInterestsAsync();
            var pageSize = 12;

            // Find users based on filtering criteria
            List<(User User, double Similarity, List<Interest> SharedInterests)> discoveredUsers;

            if (filterByMyInterests)
            {
                // Get similar users using the existing service method
                discoveredUsers = await _interestService.GetSimilarUsersWithInterestsAsync(userId, 100);
            }
            else if (interests != null && interests.Any())
            {
                // Find users with specific interests
                discoveredUsers = await GetUsersByInterestsAsync(userId, interests);
            }
            else
            {
                // Default - get a mix of users with potential interest similarity
                discoveredUsers = await _interestService.GetSimilarUsersWithInterestsAsync(userId, 100);
            }

            // Get friend information
            var friends = await _friendService.GetUserFriendsAsync(userId);
            var friendIds = friends
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();

            // Get both sent and received pending requests
            var sentRequests = await _friendService.GetSentFriendRequestsAsync(userId);
            var receivedRequests = await _friendService.GetReceivedFriendRequestsAsync(userId);
            var pendingRequestUserIds = new List<int>();

            // Add pending sent request recipient IDs
            pendingRequestUserIds.AddRange(sentRequests.Select(r => r.ReceiverId));
            // Add pending received request sender IDs
            pendingRequestUserIds.AddRange(receivedRequests.Select(r => r.SenderId));

            // Apply pagination
            var totalItems = discoveredUsers.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var paginatedUsers = discoveredUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to view model
            var userItems = new List<UserDiscoveryItemVM>();
            foreach (var userInfo in paginatedUsers)
            {
                var user = userInfo.User;
                var similarity = userInfo.Similarity;
                var sharedInterests = userInfo.SharedInterests;

                var userPosts = await _postsService.GetUserPostsAsync(user.Id, userId);
                var postContent = userPosts.FirstOrDefault()?.Content ?? string.Empty;
                var truncatedContent = !string.IsNullOrEmpty(postContent)
                    ? postContent.Substring(0, Math.Min(100, postContent.Length))
                    : string.Empty;

                userItems.Add(new UserDiscoveryItemVM
                {
                    User = user,
                    MatchPercentage = (int)similarity,
                    SharedInterests = sharedInterests,
                    IsFriend = friendIds.Contains(user.Id),
                    HasPendingFriendRequest = pendingRequestUserIds.Contains(user.Id),
                    MostRecentPost = truncatedContent,
                    PostCount = userPosts.Count
                });
            }

            var viewModel = new UserDiscoveryVM
            {
                DiscoveredUsers = userItems,
                AvailableInterests = allInterests,
                SelectedInterests = interests ?? new List<int>(),
                FilterByMyInterests = filterByMyInterests,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        private async Task<List<(User User, double Similarity, List<Interest> SharedInterests)>> GetUsersByInterestsAsync(int userId, List<int> interestIds)
        {
            var context = HttpContext.RequestServices.GetService(typeof(AppDBContext)) as AppDBContext;

            // Find users who have the selected interests
            var usersWithInterests = await context.UserInterests
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

                // Fix the tuple creation by explicitly creating the tuple with named components
                result.Add((User: user, Similarity: similarity, SharedInterests: sharedInterests));
            }

            return result.OrderByDescending(u => u.Similarity).ToList();
        }
    }
}