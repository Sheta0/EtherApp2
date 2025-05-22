using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class NotificationsController(INotificationService notificationService) : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();
            var count = await notificationService.GetUnreadNotificationsCountAsync(loggedInUser.Value);
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();
            var notifications = await notificationService.GetAllNotificationsAsync(loggedInUser.Value);
            return PartialView("Notifications/_Notifications", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] int id)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return Json(new { success = false, message = "User not authenticated" });

            try
            {
                var result = await notificationService.MarkNotificationAsReadAsync(id, loggedInUser.Value);
                if (!result)
                {
                    return Json(new { success = false, message = "Failed to mark notification as read" });
                }

                var notifications = await notificationService.GetAllNotificationsAsync(loggedInUser.Value);
                return PartialView("Notifications/_Notifications", notifications);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return Json(new { success = false, message = "User not authenticated" });

            try
            {
                await notificationService.MarkAllNotificationsAsReadAsync(loggedInUser.Value);
                var notifications = await notificationService.GetAllNotificationsAsync(loggedInUser.Value);
                return PartialView("Notifications/_Notifications", notifications);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
