using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Interests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtherApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IContentAnalysisService _contentAnalysisService;

        public AdminController(
            AppDBContext context,
            UserManager<User> userManager,
            IContentAnalysisService contentAnalysisService)
        {
            _context = context;
            _userManager = userManager;
            _contentAnalysisService = contentAnalysisService;
        }

        // Shows admin dashboard
        public async Task<IActionResult> Index()
        {
            var flaggedPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Reports)
                .Where(p => p.NrOfReports > 5 && !p.IsDeleted)
                .OrderByDescending(p => p.Reports.Count)
                .ToListAsync();
                
            foreach (var post in flaggedPosts)
            {
                post.NrOfReports = post.Reports.Count;
            }
            
            return View(flaggedPosts);
        }
        
        public IActionResult TestContentAnalysis()
        {
            return View(new ContentAnalysisViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> TestContentAnalysis(ContentAnalysisViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return View(model);
            }
            
            // Analyze the content using the service
            var analysisResults = await _contentAnalysisService.AnalyzeContentAsync(model.Content);
            
            // Get all interests to match IDs to names
            var interests = await _context.Interests.ToListAsync();
            
            // Convert the analysis results to InterestScore objects
            var results = new List<InterestScore>();
            foreach (var result in analysisResults)
            {
                var interest = interests.FirstOrDefault(i => i.Id == result.InterestId);
                if (interest != null)
                {
                    results.Add(new InterestScore
                    {
                        Interest = interest.Name,
                        Score = result.Score,
                        Keywords = interest.Keywords ?? string.Empty
                    });
                }
            }
            
            model.Results = results;
            
            return View(model);
        }
        
        // POST: Admin/ApprovePost
        [HttpPost]
        public async Task<IActionResult> ApprovePost(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.Reports)
                .FirstOrDefaultAsync(p => p.Id == postId);
                
            if (post != null)
            {
                // Remove all reports for this post
                _context.Reports.RemoveRange(post.Reports);
                post.NrOfReports = 0;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Post has been approved and all reports have been cleared.";
            }
            else
            {
                TempData["ErrorMessage"] = "Post not found.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Admin/DeletePost
        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.IsDeleted = true;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Post has been deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Post not found.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}