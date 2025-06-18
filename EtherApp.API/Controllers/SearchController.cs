using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class SearchController : BaseApiController
    {
        private readonly AppDBContext _context;
        
        public SearchController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(ApiResponse<object>.SuccessResponse(new { 
                    Query = query,
                    Users = new List<object>(),
                    Posts = new List<object>()
                }));
            }

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            // Normalize query
            query = query.ToLower().Trim();

            // Search for users
            var users = await _context.Users
                .Where(u => !u.IsDeleted &&
                    (u.UserName.ToLower().Contains(query) ||
                     u.FullName.ToLower().Contains(query) || 
                     (u.Bio != null && u.Bio.ToLower().Contains(query))))
                .Select(u => new {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.ProfilePictureUrl,
                    u.Bio
                })
                .Take(20)
                .ToListAsync();

            // Search for posts
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Like)
                .Include(p => p.Comment).ThenInclude(c => c.User)
                .Include(p => p.Favorites)
                .Include(p => p.Reports)
                .Include(p => p.Interests).ThenInclude(i => i.Interest)
                .Where(p => !p.IsPrivate && p.NrOfReports < 5 &&
                    p.Content.ToLower().Contains(query))
                .OrderByDescending(p => p.DateCreated)
                .Take(20)
                .ToListAsync();

            // Search for posts by hashtag (if query starts with #)
            if (query.StartsWith("#"))
            {
                var hashtag = query.Substring(1); // Remove # character
                var hashtagPosts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Like)
                    .Include(p => p.Comment).ThenInclude(c => c.User)
                    .Include(p => p.Favorites)
                    .Include(p => p.Reports)
                    .Include(p => p.Interests).ThenInclude(i => i.Interest)
                    .Where(p => !p.IsPrivate && p.NrOfReports < 5 &&
                        p.Content.ToLower().Contains($"#{hashtag}"))
                    .OrderByDescending(p => p.DateCreated)
                    .Take(20)
                    .ToListAsync();
                
                posts = posts.Union(hashtagPosts).Distinct().ToList();
            }

            return Ok(ApiResponse<object>.SuccessResponse(new {
                Query = query,
                Users = users,
                Posts = posts
            }));
        }
    }
}