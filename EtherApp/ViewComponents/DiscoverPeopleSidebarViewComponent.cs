using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EtherApp.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace EtherApp.ViewComponents
{
    public class DiscoverPeopleSidebarViewComponent : ViewComponent
    {
        private readonly IInterestService _interestService;
        private readonly IFriendsService _friendsService;
        private readonly UserManager<User> _userManager;

        public DiscoverPeopleSidebarViewComponent(
            IInterestService interestService,
            IFriendsService friendsService,
            UserManager<User> userManager)
        {
            _interestService = interestService;
            _friendsService = friendsService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return View(new List<(User, double, List<Interest>)>());
            }

            // Get similar users (the internal GetSimilarUsersAsync already excludes friends)
            var similarUsersWithInterests = await _interestService.GetSimilarUsersWithInterestsAsync(user.Id, 10);
            
            // But we still need to exclude users with pending requests
            // Get pending sent requests
            var sentRequests = await _friendsService.GetSentFriendRequestsAsync(user.Id);
            var sentRequestUserIds = sentRequests.Select(r => r.ReceiverId).ToList();
            
            // Get pending received requests
            var receivedRequests = await _friendsService.GetReceivedFriendRequestsAsync(user.Id);
            var receivedRequestUserIds = receivedRequests.Select(r => r.SenderId).ToList();
            
            // Combine all user IDs to exclude
            var excludeUserIds = new HashSet<int>();
            excludeUserIds.UnionWith(sentRequestUserIds);
            excludeUserIds.UnionWith(receivedRequestUserIds);
            
            // Filter out users with pending requests and take only 3
            var filteredUsers = similarUsersWithInterests
                .Where(u => !excludeUserIds.Contains(u.User.Id))
                .Take(3)
                .ToList();
            
            return View(filteredUsers);
        }
    }
}