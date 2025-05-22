using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Friends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class FriendsController(IFriendsService friendsService, INotificationService notificationService) : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var friendsData = new FriendshipVM()
            {
                FriendRequests = await friendsService.GetSentFriendRequestsAsync(userId.Value),
                ReceivedRequests = await friendsService.GetReceivedFriendRequestsAsync(userId.Value),
                Friends = await friendsService.GetUserFriendsAsync(userId.Value)
            };

            ViewBag.CurrentUserId = userId.Value;
            return View(friendsData);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(int receiverId)
        {
            var senderId = GetUserId();
            var userName = GetUserFullName();
            if (!senderId.HasValue)
                return RedirectToLogin();

            await friendsService.SendRequestAsync(senderId.Value, receiverId);
            await notificationService.AddNewNotificationAsync(receiverId, NotificationType.FriendRequest, userName, null);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequest(int requestId, string status)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return RedirectToLogin();

            var request = await friendsService.UpdateRequestAsync(requestId, status);

            if (status == FriendRequestStatus.Accepted)
                await notificationService.AddNewNotificationAsync(request.SenderId, NotificationType.FriendRequestAccepted, userName, null);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            await friendsService.RemoveFriendAsync(friendshipId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }


    }
}
