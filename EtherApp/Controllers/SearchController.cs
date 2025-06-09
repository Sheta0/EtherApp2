using EtherApp.Controllers.Base;
using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtherApp.Controllers
{
    [Authorize]
    public class SearchController : BaseController
    {
        private readonly AppDBContext _context;
        private readonly IPostsService _postsService;

        public SearchController(AppDBContext context, IPostsService postsService)
        {
            _context = context;
            _postsService = postsService;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchResultsVM { Query = query });
            }

            var loggedInUserId = GetUserId() ?? 0;
            if (loggedInUserId == 0)
                return RedirectToLogin();

            // Normalize query
            query = query.ToLower().Trim();

            // Search for users
            var users = await _context.Users
                .Where(u => !u.IsDeleted &&
                    (u.UserName.ToLower().Contains(query) ||
                     u.FullName.ToLower().Contains(query) || 
                     (u.Bio != null && u.Bio.ToLower().Contains(query))))
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
                
                // Combine with normal post results, eliminate duplicates
                posts = posts.Union(hashtagPosts).Distinct().ToList();
            }

            var viewModel = new SearchResultsVM
            {
                Query = query,
                Users = users,
                Posts = posts
            };

            return View(viewModel);
        }
    }
}