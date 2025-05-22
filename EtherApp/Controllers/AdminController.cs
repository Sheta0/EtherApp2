using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EtherApp.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController(IAdminService adminService) : Controller
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
    }
}
