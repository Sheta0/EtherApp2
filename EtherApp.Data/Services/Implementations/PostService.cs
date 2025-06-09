using EtherApp.Data.Dtos;
using EtherApp.Data.Helpers;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherApp.Data.Helpers.Enums;

namespace EtherApp.Data.Services.Implementations
{
    public class PostService(AppDBContext _context, INotificationService notificationService, IInterestService interestService) : IPostsService
    {

        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
               .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.NrOfReports < 5 /*&& !n.IsDeleted*/)
               .Include(n => n.User)
               .Include(n => n.Like)
               .Include(n => n.Favorites)
               .Include(n => n.Comment).ThenInclude(n => n.User)
               .Include(n => n.Reports)
               .Include(n => n.Interests).ThenInclude(i => i.Interest)
               .OrderByDescending(n => n.DateCreated)
               .ToListAsync();

            return allPosts;
        }

        public async Task<List<Post>?> GetUserPostsAsync(int userId, int loggedInUserId)
        {
            var userPosts = await _context.Posts
                .Where(n => n.UserId == userId &&
                          (!n.IsPrivate || n.UserId == loggedInUserId) &&
                          n.NrOfReports < 5)
                .Include(n => n.User)
                .Include(n => n.Like)
                .Include(n => n.Favorites)
                .Include(n => n.Comment).ThenInclude(n => n.User)
                .Include(n => n.Reports)
                .Include(n => n.Interests).ThenInclude(i => i.Interest)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return userPosts;
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var postDb = await _context.Posts
                .Include(n => n.User)
                .Include(n => n.Like)
                .Include(n => n.Favorites)
                .Include(n => n.Interests).ThenInclude(i => i.Interest)
                .Include(n => n.Comment).ThenInclude(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == postId);
            return postDb;
        }

        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            var allFavoritedPosts = await _context.Favorites
                .Where(n => n.UserId == loggedInUserId && n.Post.Reports.Count < 5 /*&& !n.IsDeleted*/)
                .Include(f => f.Post)
                    .ThenInclude(p => p.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Favorites)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Comment)
                        .ThenInclude(c => c.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Like)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Interests).ThenInclude(i => i.Interest)
                .Select(f => f.Post)
                .ToListAsync();

            return allFavoritedPosts;
        }

        public async Task<Comment> AddPostCommontAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();


            await interestService.UpdateUserInterestWeightsAsync(comment.UserId, comment.PostId, InteractionType.Comment);

            return comment;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            // Process interests
            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await interestService.ProcessPostInterestsAsync(post.Id, post.Content);
                
                // Add this line to update the user's own interests based on their post
                await interestService.UpdateUserInterestWeightsAsync(post.UserId, post.Id, InteractionType.Create);
            }

            return post;
        }

        public async Task<Post> DeletePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);

            if (postDb != null)
            {

                _context.Posts.Remove(postDb);
                await _context.SaveChangesAsync();

            }

            return postDb;
        }

        public async Task DeletePostCommentAsync(int commentId)
        {
            var commentDb = _context.Comments.FirstOrDefault(n => n.Id == commentId);

            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report()
            {
                UserId = userId,
                PostId = postId,
                DateCreated = DateTime.Now
            };

            await _context.Reports.AddAsync(newReport);

            // Update the NrOfReports in the Posts table
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post != null)
            {
                post.NrOfReports++;
                _context.Posts.Update(post);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<GetNotificationDto> TogglePostFavoriteAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            // Check if user favorited the post
            var favorite = await _context.Favorites
                .Where(f => f.PostId == postId && f.UserId == userId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.Now
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();

                // Update user interest weights
                await interestService.UpdateUserInterestWeightsAsync(userId, postId, InteractionType.Favorite);

                response.SendNotification = true;
            }

            return response;
        }

        public async Task<GetNotificationDto> TogglePostLikeAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            // Check if user liked the post
            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();

                // Update user interest weights
                await interestService.UpdateUserInterestWeightsAsync(userId, postId, InteractionType.Like);

                response.SendNotification = true;
            }

            return response;
        }


        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            //check if user favorited the post
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);
            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetRecommendedPostsAsync(int userId, int count = 10)
        {
            // Get user interests
            var userInterests = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .ToDictionaryAsync(ui => ui.InterestId, ui => ui.Weight);

            if (!userInterests.Any())
            {
                // If user has no interests yet, return recent posts (excluding own posts)
                return await _context.Posts
                    .Where(p => !p.IsPrivate && p.UserId != userId) // Exclude own posts
                    .OrderByDescending(p => p.DateCreated)
                    .Take(count)
                    .Include(p => p.User)
                    .Include(p => p.Like)
                    .Include(p => p.Comment).ThenInclude(c => c.User)
                    .Include(p => p.Favorites)
                    .Include(p => p.Reports)
                    .Include(p => p.Interests).ThenInclude(i => i.Interest)
                    .ToListAsync();
            }

            // Get all public posts with their interest scores (excluding own posts)
            var posts = await _context.Posts
                .Where(p => !p.IsPrivate && p.UserId != userId) // Exclude own posts
                .Include(p => p.User)
                .Include(p => p.Like)
                .Include(p => p.Comment).ThenInclude(c => c.User)
                .Include(p => p.Favorites)
                .Include(p => p.Reports)
                .Include(p => p.Interests).ThenInclude(i => i.Interest)
                .ToListAsync();

            // Calculate relevance score for each post based on user interests
            var scoredPosts = posts.Select(post =>
            {
                double relevanceScore = 0;

                foreach (var postInterest in post.Interests)
                {
                    if (userInterests.TryGetValue(postInterest.InterestId, out double weight))
                    {
                        relevanceScore += postInterest.Score * weight;
                    }
                }

                // Add a recency factor (posts from last 24 hours get a boost)
                if ((DateTime.Now - post.DateCreated).TotalHours <= 24)
                {
                    relevanceScore *= 1.5;
                }

                return new { Post = post, Score = relevanceScore };
            })
            .OrderByDescending(p => p.Score)
            .Take(count)
            .Select(p => p.Post)
            .ToList();

            return scoredPosts;
        }
    }
}
