using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddNewNotificationAsync(int userId, string notificationType, string userFullName, int? postId);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<List<Notification>> GetAllNotificationsAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<int> MarkAllNotificationsAsReadAsync(int userId);
    }
}
