using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Services;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Interests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController(IAdminService adminService, IContentAnalysisService contentAnalysisService, IInterestService interestService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var reportedPosts = await adminService.GetReportedPostsAsync();
            return View(reportedPosts);
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePost(int postId)
        {
            var result = await adminService.ApprovePostAsync(postId);
            if (result)
            {
                TempData["SuccessMessage"] = "Post approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve post.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var result = await adminService.DeletePostAsync(postId);
            if (result)
            {
                TempData["SuccessMessage"] = "Post deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete post.";
            }
            return RedirectToAction("Index");
        }

        // Test Content Analysis actions
        [HttpGet]
        public IActionResult TestContentAnalysis()
        {
            return View(new ContentAnalysisViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> TestContentAnalysis(ContentAnalysisViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Content))
            {
                var scores = await contentAnalysisService.AnalyzeContentAsync(model.Content);
                var interestsList = await interestService.GetAllInterestsAsync();
                var interests = interestsList.ToDictionary(i => i.Id);

                model.Results = scores.Select(s => new InterestScore
                {
                    Interest = interests[s.InterestId].Name,
                    Score = s.Score,
                    Keywords = interests[s.InterestId].Keywords
                }).ToList();
            }

            return View(model);
        }
    }

}
