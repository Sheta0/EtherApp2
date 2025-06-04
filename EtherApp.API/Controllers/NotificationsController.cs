using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public NotificationsController(
            INotificationService notificationService,
            UserManager<User> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notifications = await _notificationService.GetAllNotificationsAsync(user.Id);
            return Ok(notifications);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadNotificationsAsync(user.Id);
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound();

            if (notification.UserId != user.Id)
                return Forbid();

            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(user.Id);
            return Ok();
        }
    }
}