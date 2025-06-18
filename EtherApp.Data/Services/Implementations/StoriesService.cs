using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Implementations
{
    public class StoriesService : IStoriesService
    {
        private readonly AppDBContext _context;
        public StoriesService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Story>> GetAllStoriesAsync()
        {
            var allStories = await _context.Stories
                  .Where(n => n.DateCreated > DateTime.Now.AddDays(-1))
                  .Include(s => s.User)
                  .ToListAsync();

            return allStories;
        }
        
        public async Task<Story> CreateStoryAsync(Story story)
        {
            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();

            return story;
        }
        
        public async Task<List<Story>> GetUserStoriesAsync(int userId)
        {
            var userStories = await _context.Stories
                .Where(s => s.UserId == userId && 
                           !s.IsDeleted && 
                           s.DateCreated > DateTime.Now.AddDays(-1))
                .Include(s => s.User)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();
            
            return userStories;
        }

        public async Task<List<Story>> GetUserAndFriendStoriesAsync(int userId)
        {
            // Get all friendships where the current user is either sender or receiver
            var friendships = await _context.Friendships
                .Where(f => f.SenderId == userId || f.ReceiverId == userId)
                .ToListAsync();

            // Extract friend IDs
            var friendIds = friendships
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();
            
            // Add current user to the list
            friendIds.Add(userId);

            // Get stories from the user and their friends from the last 24 hours
            var stories = await _context.Stories
                .Where(s => friendIds.Contains(s.UserId) && 
                           !s.IsDeleted && 
                           s.DateCreated > DateTime.Now.AddDays(-1))
                .Include(s => s.User)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();

            return stories;
        }
    }
}
