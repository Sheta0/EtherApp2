using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Friends;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.ViewComponents
{
    public class SuggestedFriendsViewComponent(IFriendsService friendsService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loggedInUser = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            var loggedInUserId = int.Parse(loggedInUser);
            var suggestedFriends = await friendsService.GetSuggestedFriendsAsync(loggedInUserId);

            var suggestedFriendsVM = new List<UserWithFriendsCountVM>();

            foreach (var u in suggestedFriends)
            {
                var pendingRequest = await friendsService.GetFriendRequestAsync(loggedInUserId, u.User.Id);

                suggestedFriendsVM.Add(new UserWithFriendsCountVM
                {
                    UserId = u.User.Id,
                    FullName = u.User.FullName,
                    ProfilePictureUrl = u.User.ProfilePictureUrl,
                    FriendsCount = u.FriendsCount,
                    PendingFriendRequest = pendingRequest,
                    HasSentRequest = pendingRequest != null && pendingRequest.SenderId == loggedInUserId
                });
            }

            return View(suggestedFriendsVM);
        }

    }
}
