using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class FriendsController : BaseApiController
    {
        private readonly IFriendsService _friendsService;
        private readonly INotificationService _notificationService;
        
        public FriendsController(IFriendsService friendsService, INotificationService notificationService)
        {
            _friendsService = friendsService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsData()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var friendsData = new
            {
                SentRequests = await _friendsService.GetSentFriendRequestsAsync(userId.Value),
                ReceivedRequests = await _friendsService.GetReceivedFriendRequestsAsync(userId.Value),
                Friends = await _friendsService.GetUserFriendsAsync(userId.Value)
            };

            return Ok(ApiResponse<object>.SuccessResponse(friendsData));
        }

        [HttpPost("request/{receiverId}")]
        public async Task<IActionResult> SendFriendRequest(int receiverId)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            await _friendsService.SendRequestAsync(userId.Value, receiverId);
            await _notificationService.AddNewNotificationAsync(receiverId, NotificationType.FriendRequest, userName, null);

            return Ok(ApiResponse<object>.SuccessResponse("Friend request sent successfully"));
        }

        [HttpPut("request/{requestId}")]
        public async Task<IActionResult> UpdateFriendRequest(int requestId, [FromBody] UpdateFriendRequestModel model)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var request = await _friendsService.UpdateRequestAsync(requestId, model.Status);

            if (model.Status == FriendRequestStatus.Accepted)
                await _notificationService.AddNewNotificationAsync(request.SenderId, NotificationType.FriendRequestAccepted, userName, null);

            return Ok(ApiResponse<object>.SuccessResponse("Friend request updated successfully"));
        }

        [HttpDelete("friendship/{friendshipId}")]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            await _friendsService.RemoveFriendAsync(friendshipId);
            return Ok(ApiResponse<object>.SuccessResponse("Friend removed successfully"));
        }
    }

    public class UpdateFriendRequestModel
    {
        public string Status { get; set; }
    }
}