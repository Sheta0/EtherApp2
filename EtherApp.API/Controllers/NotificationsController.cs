using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;
        
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var notifications = await _notificationService.GetAllNotificationsAsync(userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(notifications));
        }

        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var count = await _notificationService.GetUnreadNotificationsCountAsync(userId.Value);
            return Ok(ApiResponse<int>.SuccessResponse(count));
        }

        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(id, userId.Value);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Failed to mark notification as read"));
                }

                return Ok(ApiResponse<object>.SuccessResponse("Notification marked as read"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("read/all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            try
            {
                await _notificationService.MarkAllNotificationsAsReadAsync(userId.Value);
                return Ok(ApiResponse<object>.SuccessResponse("All notifications marked as read"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}