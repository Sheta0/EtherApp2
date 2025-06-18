using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Helpers
{
    public static class DBInitializer
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // Roles
            if (!roleManager.Roles.Any())
            {
                foreach (var role in AppRoles.AllRoles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>(role));
                    }
                }
            }

            // Users with roles
            if (!userManager.Users.Any(n => !string.IsNullOrEmpty(n.Email)))
            {
                var userPassword = "P@ssw0rd";
                var user = new User()
                {
                    UserName = "sheta0.9",
                    Email = "theimpossible000@gmail.com",
                    FullName = "Ahmed Sheta",
                    Bio = "Original developer of EtherApp. I enjoy tech innovations and coding.",
                    ProfilePictureUrl = "images/avatar/User.jpg",
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(user, userPassword);

                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, AppRoles.User);
                }

                var Admin = new User()
                {
                    UserName = "admin.admin",
                    Email = "ahmedsheta834@gmail.com",
                    FullName = "Sheta Admin",
                    Bio = "Admin account for system management.",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };

                var adminResult = await userManager.CreateAsync(Admin, userPassword);

                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(Admin, AppRoles.Admin);
                }

                // Test Users with different interests
                // Tech Enthusiast
                var techUser = new User()
                {
                    UserName = "techguru",
                    Email = "tech@example.com",
                    FullName = "Alex Tech",
                    Bio = "Software engineer with passion for AI and emerging technologies. Always learning!",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(techUser, "Test123!");
                await userManager.AddToRoleAsync(techUser, AppRoles.User);

                // Arts & Music Lover
                var artsUser = new User()
                {
                    UserName = "artsylover",
                    Email = "arts@example.com",
                    FullName = "Jamie Creative",
                    Bio = "Musician, painter, and photography enthusiast. Art is my way of expressing emotions.",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(artsUser, "Test123!");
                await userManager.AddToRoleAsync(artsUser, AppRoles.User);

                // Sports & Fitness Enthusiast
                var sportsUser = new User()
                {
                    UserName = "fitlife",
                    Email = "sports@example.com",
                    FullName = "Sam Athletic",
                    Bio = "Marathon runner, basketball player and fitness trainer. Health and sports are my passion!",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(sportsUser, "Test123!");
                await userManager.AddToRoleAsync(sportsUser, AppRoles.User);

                // Food & Travel Blogger
                var foodTravelUser = new User()
                {
                    UserName = "wanderlust",
                    Email = "travel@example.com",
                    FullName = "Morgan Foodie",
                    Bio = "Exploring the world one dish at a time. Food blogger and travel addict.",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(foodTravelUser, "Test123!");
                await userManager.AddToRoleAsync(foodTravelUser, AppRoles.User);

                // Business & Education Professional
                var businessUser = new User()
                {
                    UserName = "bizpro",
                    Email = "business@example.com",
                    FullName = "Taylor Business",
                    Bio = "Business consultant, educator, and startup mentor. Passionate about entrepreneurship.",
                    ProfilePictureUrl = "images/avatar/User.png",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(businessUser, "Test123!");
                await userManager.AddToRoleAsync(businessUser, AppRoles.User);
            }
        }

        public static async Task SeedAsync(AppDBContext appDBContext, IInterestService interestService)
        {
            // Seed interests for users first
            await SeedUserInterestsAsync(appDBContext, interestService);

            // Check if there are already posts in the database
            if (!appDBContext.Posts.Any())
            {
                var users = appDBContext.Users.ToList();
                var posts = new List<Post>();
                var now = DateTime.Now;

                // Tech posts (keywords: tech, computer, software, programming, code, developer, app)
                var techUser = users.FirstOrDefault(u => u.UserName == "techguru");
                if (techUser != null)
                {
                    posts.AddRange(new List<Post>
                    {
                        new Post
                        {
                            Content = "Just finished building my first machine learning model! The capabilities of TensorFlow are incredible. Anyone else working on programming AI projects? Learning to code these algorithms has been fascinating.",
                            DateCreated = now.AddDays(-5),
                            DateUpdated = now.AddDays(-5),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = techUser.Id
                        },
                        new Post
                        {
                            Content = "Spent the weekend optimizing my code and improving app performance. Clean software is happy software! As a developer, I find these tech challenges so rewarding.",
                            DateCreated = now.AddDays(-3),
                            DateUpdated = now.AddDays(-3),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = techUser.Id
                        },
                        new Post
                        {
                            Content = "The new computer processors are revolutionizing tech. I've been testing some benchmarks and the results are mind-blowing! Software runs so much faster now.",
                            DateCreated = now.AddDays(-1),
                            DateUpdated = now.AddDays(-1),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = techUser.Id
                        }
                    });

                    
                }

                // Arts posts (keywords: art, paint, drawing, design, creative, artist, music, song, band, concert, album, guitar, piano)
                var artsUser = users.FirstOrDefault(u => u.UserName == "artsylover");
                if (artsUser != null)
                {
                    posts.AddRange(new List<Post>
                    {
                        new Post
                        {
                            Content = "Just finished my latest painting! Experimenting with watercolor techniques has been so rewarding. The art community here is amazing. As an artist I find inspiration everywhere.",
                            DateCreated = now.AddDays(-6),
                            DateUpdated = now.AddDays(-6),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = artsUser.Id
                        },
                        new Post
                        {
                            Content = "Went to an amazing concert last night. The band played such wonderful songs! Music really feeds the soul. The guitar and piano duet was particularly moving.",
                            DateCreated = now.AddDays(-4),
                            DateUpdated = now.AddDays(-4),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = artsUser.Id
                        },
                        new Post
                        {
                            Content = "Started learning guitar this week. My fingers hurt but I'm already in love with the creative process! The album I'm trying to learn from is quite challenging.",
                            DateCreated = now.AddDays(-1),
                            DateUpdated = now.AddDays(-1),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = artsUser.Id
                        }
                    });

                }

                // Sports posts (keywords: sport, team, game, play, athlete, fitness, exercise, health, wellness)
                var sportsUser = users.FirstOrDefault(u => u.UserName == "fitlife");
                if (sportsUser != null)
                {
                    posts.AddRange(new List<Post>
                    {
                        new Post
                        {
                            Content = "Completed my first marathon today! So proud of this fitness achievement. The team of athletes running alongside me was so supportive throughout the exercise.",
                            DateCreated = now.AddDays(-7),
                            DateUpdated = now.AddDays(-7),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = sportsUser.Id
                        },
                        new Post
                        {
                            Content = "New workout routine is really helping my wellness journey! Focus on functional fitness is really changing my health and strength levels. Exercise is truly medicine.",
                            DateCreated = now.AddDays(-3),
                            DateUpdated = now.AddDays(-3),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = sportsUser.Id
                        },
                        new Post
                        {
                            Content = "That basketball game last night was incredible! The sport at its finest! Our team played exceptionally well, and I'm looking forward to the next game with these athletes.",
                            DateCreated = now.AddDays(-1),
                            DateUpdated = now.AddDays(-1),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = sportsUser.Id
                        }
                    });

                }

                // Food & Travel posts (keywords: travel, trip, vacation, destination, journey, explore, food, recipe, cook, bake, meal, restaurant, dish)
                var foodTravelUser = users.FirstOrDefault(u => u.UserName == "wanderlust");
                if (foodTravelUser != null)
                {
                    posts.AddRange(new List<Post>
                    {
                        new Post
                        {
                            Content = "Finally visited Japan on my latest travel adventure! The food in Tokyo is mind-blowing. Every restaurant I visited offered an amazing meal. This destination was worth the journey!",
                            DateCreated = now.AddDays(-10),
                            DateUpdated = now.AddDays(-10),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = foodTravelUser.Id
                        },
                        new Post
                        {
                            Content = "Tried a new recipe today - made pasta from scratch! The dish turned out so much better than I expected. Learning to cook like this has been so rewarding.",
                            DateCreated = now.AddDays(-4),
                            DateUpdated = now.AddDays(-4),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = foodTravelUser.Id
                        },
                        new Post
                        {
                            Content = "Planning my next vacation to Southeast Asia. Looking for interesting destinations to explore in Thailand and Vietnam. This trip will focus on local food and authentic meal experiences.",
                            DateCreated = now.AddDays(-1),
                            DateUpdated = now.AddDays(-1),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = foodTravelUser.Id
                        }
                    });

                    
                }

                // Business posts (keywords: business, company, startup, entrepreneur, market, invest, education)
                var businessUser = users.FirstOrDefault(u => u.UserName == "bizpro");
                if (businessUser != null)
                {
                    posts.AddRange(new List<Post>
                    {
                        new Post
                        {
                            Content = "Just finished mentoring a startup session. So impressed by the innovation and entrepreneurial spirit in these young founders! The business ideas were fantastic.",
                            DateCreated = now.AddDays(-8),
                            DateUpdated = now.AddDays(-8),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = businessUser.Id
                        },
                        new Post
                        {
                            Content = "Reading about market trends and how they affect company growth strategies. As an entrepreneur, I find these business insights valuable for future investments.",
                            DateCreated = now.AddDays(-5),
                            DateUpdated = now.AddDays(-5),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = businessUser.Id
                        },
                        new Post
                        {
                            Content = "Giving a lecture at the university tomorrow on sustainable business practices. Education is key to developing the next generation of business leaders!",
                            DateCreated = now.AddDays(-1),
                            DateUpdated = now.AddDays(-1),
                            IsPrivate = false,
                            NrOfReports = 0,
                            UserId = businessUser.Id
                        }
                    });

                    
                }

                // Add all posts to database
                await appDBContext.Posts.AddRangeAsync(posts);
                await appDBContext.SaveChangesAsync();

                // Process post interests based on content
                foreach (var post in posts)
                {
                    await interestService.ProcessPostInterestsAsync(post.Id, post.Content);
                }
            }
        }

        private static async Task SeedUserInterestsAsync(AppDBContext appDBContext, IInterestService interestService)
        {
            // Skip if already seeded
            if (appDBContext.UserInterests.Any())
                return;

            var users = appDBContext.Users.ToList();
            var interests = await interestService.GetAllInterestsAsync();
            if (!interests.Any())
                return; // Can't proceed if we don't have interests

            // Tech user interests
            var techUser = users.FirstOrDefault(u => u.UserName == "techguru");
            if (techUser != null)
            {
                var techInterests = interests
                    .Where(i => new[] { 
                        "Technology", "Programming", "Artificial Intelligence", "Computer Science", 
                        "Software Development" 
                    }.Contains(i.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();
                
                if (techInterests.Any())
                    await interestService.UpdateUserInterestsAsync(techUser.Id, techInterests);
            }

            // Arts user interests
            var artsUser = users.FirstOrDefault(u => u.UserName == "artsylover");
            if (artsUser != null)
            {
                var artInterests = interests
                    .Where(i => new[] { 
                        "Art", "Music", "Painting", "Drawing", "Design", "Creativity" 
                    }.Contains(i.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();
                
                if (artInterests.Any())
                    await interestService.UpdateUserInterestsAsync(artsUser.Id, artInterests);
            }

            // Sports user interests
            var sportsUser = users.FirstOrDefault(u => u.UserName == "fitlife");
            if (sportsUser != null)
            {
                var sportsInterests = interests
                    .Where(i => new[] { 
                        "Sports", "Fitness", "Health", "Exercise", "Basketball", "Running" 
                    }.Contains(i.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();
                
                if (sportsInterests.Any())
                    await interestService.UpdateUserInterestsAsync(sportsUser.Id, sportsInterests);
            }

            // Food & Travel user interests
            var foodTravelUser = users.FirstOrDefault(u => u.UserName == "wanderlust");
            if (foodTravelUser != null)
            {
                var travelFoodInterests = interests
                    .Where(i => new[] { 
                        "Travel", "Food", "Cooking", "Culinary", "Exploration", "Culture" 
                    }.Contains(i.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();
                
                if (travelFoodInterests.Any())
                    await interestService.UpdateUserInterestsAsync(foodTravelUser.Id, travelFoodInterests);
            }

            // Business user interests
            var businessUser = users.FirstOrDefault(u => u.UserName == "bizpro");
            if (businessUser != null)
            {
                var businessInterests = interests
                    .Where(i => new[] { 
                        "Business", "Entrepreneurship", "Education", "Investment", 
                        "Marketing", "Leadership" 
                    }.Contains(i.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(i => i.Id)
                    .ToList();
                
                if (businessInterests.Any())
                    await interestService.UpdateUserInterestsAsync(businessUser.Id, businessInterests);
            }
        }
    }
}
