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
    public class UserService : IUserService
    {
        private readonly AppDBContext _appDbContext;
        public UserService(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<User> GetUserByIdAsync(int loggedInUserId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId) ?? new User() ;
        }

        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Id == loggedInUserId);

            if (user != null)
            {
                user.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(user);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetUserPosts(int userId, int loggedInUserId)
        {
            return await _appDbContext.Posts
               .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.NrOfReports < 5 && n.UserId == userId /*&& !n.IsDeleted*/)
               .Include(n => n.User)
               .Include(n => n.Like)
               .Include(n => n.Favorites)
               .Include(n => n.Comment).ThenInclude(n => n.User)
               .Include(n => n.Reports)
               .OrderByDescending(n => n.DateCreated)
               .ToListAsync();
        }
    }
}
