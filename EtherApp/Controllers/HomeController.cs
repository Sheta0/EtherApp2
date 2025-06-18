using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace EtherApp.Controllers
{
    [Authorize]
    public class HomeController(
        IPostsService _postsService,
        IHashtagsService _hashtagsService,
        IFilesService _filesService,
        INotificationService notificationService,
        IInterestService interestService) : BaseController
    {


        public async Task<IActionResult> Index()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();
            var allPosts = await _postsService.GetAllPostsAsync(loggedInUser.Value);
            return View(allPosts);
        }

        public async Task<IActionResult> PostDetails(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            post.Content ??= string.Empty;

            var imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            if (string.IsNullOrWhiteSpace(post.Content) && string.IsNullOrEmpty(imageUploadPath))
            {
                TempData["ErrorMessage"] = "Please provide either text content or an image for your post.";
                return RedirectToAction("Index");
            }

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUser.Value
            };

            var createdPost = await _postsService.CreatePostAsync(newPost);

            // Only process hashtags if content is not empty
            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content, loggedInUser.Value);

                // Update user interests based on their own post content
                // This treats creating content as a strong signal of interest
                await interestService.UpdateUserInterestWeightsAsync(
                    loggedInUser.Value,
                    createdPost.Id,
                    InteractionType.Create);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostLike(PostLikeVM postLikeVM)
        {
            var loggedInUser = GetUserId();
            var userName = GetUserFullName();
            if (loggedInUser is null) return RedirectToLogin();

            var result = await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUser.Value);

            var post = await _postsService.GetPostByIdAsync(postLikeVM.PostId);

            if (result.SendNotification && loggedInUser != post.UserId)
                await notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Like, userName, postLikeVM.PostId);


            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            var loggedInUser = GetUserId();
            var userName = GetUserFullName();
            if (loggedInUser is null) return RedirectToLogin();

            var result = await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId, loggedInUser.Value);

            var post = await _postsService.GetPostByIdAsync(postFavoriteVM.PostId);

            if (result.SendNotification && loggedInUser != post.UserId)
                await notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Favorite, userName, postFavoriteVM.PostId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Home/_Post", post);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.TogglePostVisibilityAsync(postVisibilityVM.PostId, loggedInUser.Value);

            var post = await _postsService.GetPostByIdAsync(postVisibilityVM.PostId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, isPrivate = post.IsPrivate });
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {
            var loggedInUser = GetUserId();
            var userName = GetUserFullName();
            if (loggedInUser is null) return RedirectToLogin();

            var newComment = new Comment()
            {
                UserId = loggedInUser.Value,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };

            await _postsService.AddPostCommontAsync(newComment);

            var post = await _postsService.GetPostByIdAsync(postCommentVM.PostId);
            if (loggedInUser != post.UserId)
                await notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Comment, userName, postCommentVM.PostId);


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Home/_Post", post);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUser.Value);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            await _postsService.DeletePostCommentAsync(removeCommentVM.CommentId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDelete(PostDeleteVM postDeleteVM)
        {
            var postRemoved = await _postsService.DeletePostAsync(postDeleteVM.PostId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content, postRemoved.UserId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetPostCommentCount(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return Content(post.Comment.Count.ToString());
        }
    }
}