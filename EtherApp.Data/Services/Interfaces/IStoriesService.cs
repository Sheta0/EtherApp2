using EtherApp.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Interfaces
{
    public interface IStoriesService
    {
        Task<List<Story>> GetUserStoriesAsync(int userId);
        Task<List<Story>> GetAllStoriesAsync();
        Task<Story> CreateStoryAsync(Story story);
        Task<List<Story>> GetUserAndFriendStoriesAsync(int userId);
    }
}
