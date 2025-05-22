using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Hubs;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public class NotificationService(AppDBContext context, IHubContext<NotificationHub> hubContext) : INotificationService
    {
        public async Task AddNewNotificationAsync(int userId, string notificationType, string userFullName, int? postId)
        {
            var newNotification = new Notification
            {
                UserId = userId,
                Message = GetPostMessage(notificationType, userFullName),
                Type = notificationType,
                IsRead = false,
                PostId = postId.HasValue ? postId.Value : null,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            await context.Notifications.AddAsync(newNotification);
            await context.SaveChangesAsync();


            var notificationCount = await GetUnreadNotificationsCountAsync(userId);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notificationCount);

        }

        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            var count = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return count;
        }

        public async Task<List<Notification>> GetAllNotificationsAsync(int userId)
        {
            var notifications = await context.Notifications
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.DateCreated)
                .ToListAsync();
            return notifications;
        }
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.DateUpdated = DateTime.UtcNow;

            context.Notifications.Update(notification);
            await context.SaveChangesAsync();

            var notificationCount = await GetUnreadNotificationsCountAsync(userId);
            await hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notificationCount);

            return true;
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(int userId)
        {
            var unreadNotifications = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return 0;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.DateUpdated = DateTime.UtcNow;
            }

            context.Notifications.UpdateRange(unreadNotifications);
            await context.SaveChangesAsync();

            await hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", 0);

            return unreadNotifications.Count;
        }


        private string GetPostMessage(string notificationType, string userFullName)
        {
            var message = "";
            switch (notificationType)
            {
                case NotificationType.Like:
                    message = $"{userFullName} liked your post.";
                    break;
                case NotificationType.Comment:
                    message = $"{userFullName} commented on your post.";
                    break;
                case NotificationType.Favorite:
                    message = $"{userFullName} favorited your post.";
                    break;
                case NotificationType.FriendRequest:
                    message = $"{userFullName} sent you a friend request.";
                    break;
                case NotificationType.FriendRequestAccepted:
                    message = $"{userFullName} accepted your friend request.";
                    break;
                default:
                    message = "You have a new notification.";
                    break;
            }

            return message;
        }

    }
}
