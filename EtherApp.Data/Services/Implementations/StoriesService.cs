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


    }
}
