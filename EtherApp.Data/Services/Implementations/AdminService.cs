using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Implementations
{
    public class AdminService(AppDBContext context) : IAdminService
    {
        public async Task<List<Post>> GetReportedPostsAsync()
        {
            var posts = await context.Posts.Include(n => n.User).Where(p => p.NrOfReports > 5).ToListAsync();

            return posts;
        }

        public async Task<bool> ApprovePostAsync(int postId)
        {
            try
            {
                var post = await context.Posts.FindAsync(postId);
                if (post == null) return false;

                var reports = await context.Reports.Where(r => r.PostId == postId).ToListAsync();
                if (reports.Any())
                {
                    context.Reports.RemoveRange(reports);
                }
                post.NrOfReports = 0;
                post.DateUpdated = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            try
            {
                var post = await context.Posts.FindAsync(postId);
                if (post == null) return false;

                var notifications = await context.Notifications.Where(n => n.PostId == postId).ToListAsync();
                if (notifications.Any())
                {
                    context.Notifications.RemoveRange(notifications);
                }

                context.Posts.Remove(post);

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
