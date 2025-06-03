using EtherApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.Data
{
    public class AppDBContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<UserHashtag> UserHashtags { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<PostInterest> PostInterests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure many-to-many relationship for UserInterest
            modelBuilder.Entity<UserInterest>()
                .HasKey(ui => new { ui.UserId, ui.InterestId });

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.Interests)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.Interest)
                .WithMany(i => i.UserInterests)
                .HasForeignKey(ui => ui.InterestId);

            // Configure many-to-many relationship for PostInterest
            modelBuilder.Entity<PostInterest>()
                .HasKey(pi => new { pi.PostId, pi.InterestId });

            modelBuilder.Entity<PostInterest>()
                .HasOne(pi => pi.Post)
                .WithMany(p => p.Interests)
                .HasForeignKey(pi => pi.PostId);

            modelBuilder.Entity<PostInterest>()
                .HasOne(pi => pi.Interest)
                .WithMany(i => i.PostInterests)
                .HasForeignKey(pi => pi.InterestId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Stories)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.PostId, l.UserId });

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Like)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Like)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
               .HasOne(l => l.Post)
               .WithMany(p => p.Comment)
               .HasForeignKey(l => l.PostId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(l => l.User)
                .WithMany(u => u.Comment)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.PostId, f.UserId });

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Post)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasKey(f => new { f.PostId, f.UserId });

            modelBuilder.Entity<Report>()
                .HasOne(f => f.Post)
                .WithMany(p => p.Reports)
                .HasForeignKey(f => f.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(f => f.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserHashtag>()
                .HasKey(uh => new { uh.UserId, uh.HashtagId });

            modelBuilder.Entity<UserHashtag>()
                .HasOne(uh => uh.User)
                .WithMany(u => u.UserHashtags)
                .HasForeignKey(uh => uh.UserId);
                

            modelBuilder.Entity<UserHashtag>()
                .HasOne(uh => uh.Hashtag)
                .WithMany(h => h.UserHashtags)
                .HasForeignKey(uh => uh.HashtagId)
                .OnDelete(DeleteBehavior.Cascade);



            base.OnModelCreating(modelBuilder);

            // Customize AspNet Identity tables Names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");


            // Friendship Configurations
            modelBuilder.Entity<Friendship>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friendship>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);


            // Seed initial interests
            SeedInterests(modelBuilder);

        }

        // In your SeedInterests method, update with keywords:
        private void SeedInterests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interest>().HasData(
                new Interest
                {
                    Id = 1,
                    Name = "Technology",
                    Description = "Computing, software, gadgets",
                    IconName = "laptop-outline",
                    Keywords = "tech,computer,software,programming,code,developer,app,AI,artificial intelligence,digital,innovation,hardware"
                },
                new Interest
                {
                    Id = 2,
                    Name = "Science",
                    Description = "Research, discoveries, nature",
                    IconName = "flask-outline",
                    Keywords = "science,research,study,experiment,lab,discovery,physics,biology,chemistry,astronomy,hypothesis,theory"
                },
                new Interest
                {
                    Id = 3,
                    Name = "Art",
                    Description = "Visual arts, design, photography",
                    IconName = "color-palette-outline",
                    Keywords = "art,paint,drawing,design,creative,artist,sketch,canvas,gallery,exhibition,sculpture,illustration"
                },
                // Continue with the rest of your interests, adding keywords to each
                new Interest
                {
                    Id = 4,
                    Name = "Music",
                    Description = "All genres, instruments, concerts",
                    IconName = "musical-notes-outline",
                    Keywords = "music,song,band,concert,album,guitar,piano,lyrics,melody,rhythm,instrument,performer,singer"
                },
                new Interest
                {
                    Id = 5,
                    Name = "Sports",
                    Description = "Athletics, teams, fitness",
                    IconName = "football-outline",
                    Keywords = "sport,team,game,play,athlete,fitness,exercise,competition,match,tournament,championship,league"
                },
                new Interest
                {
                    Id = 6,
                    Name = "Travel",
                    Description = "Destinations, adventures, tourism",
                    IconName = "airplane-outline",
                    Keywords = "travel,trip,vacation,destination,journey,explore,tourism,sightseeing,adventure,backpacking,resort,hotel"
                },
                new Interest
                {
                    Id = 7,
                    Name = "Food",
                    Description = "Cooking, recipes, restaurants",
                    IconName = "restaurant-outline",
                    Keywords = "food,recipe,cook,bake,meal,restaurant,dish,cuisine,ingredient,flavor,culinary,chef,gastronomy"
                },
                new Interest
                {
                    Id = 8,
                    Name = "Fashion",
                    Description = "Clothing, style, trends",
                    IconName = "shirt-outline",
                    Keywords = "fashion,style,clothes,outfit,trend,wear,designer,model,runway,collection,accessory,boutique"
                },
                new Interest
                {
                    Id = 9,
                    Name = "Gaming",
                    Description = "Video games, board games",
                    IconName = "game-controller-outline",
                    Keywords = "game,gaming,player,console,play,level,videogame,boardgame,roleplaying,strategy,puzzle,esports"
                },
                new Interest
                {
                    Id = 10,
                    Name = "Books",
                    Description = "Literature, authors, reading",
                    IconName = "book-outline",
                    Keywords = "book,read,author,novel,story,literature,fiction,nonfiction,biography,poetry,publish,chapter"
                },
                new Interest
                {
                    Id = 11,
                    Name = "Movies",
                    Description = "Films, cinema, directors",
                    IconName = "film-outline",
                    Keywords = "movie,film,cinema,actor,director,watch,scene,screenplay,Hollywood,blockbuster,indie,documentary"
                },
                new Interest
                {
                    Id = 12,
                    Name = "Health",
                    Description = "Wellness, fitness, medicine",
                    IconName = "fitness-outline",
                    Keywords = "health,wellness,medical,doctor,exercise,diet,nutrition,therapy,mindfulness,medicine,workout,vitality"
                },
                new Interest
                {
                    Id = 13,
                    Name = "Business",
                    Description = "Entrepreneurship, startups",
                    IconName = "briefcase-outline",
                    Keywords = "business,company,startup,entrepreneur,market,invest,finance,economy,strategy,management,leadership,innovation"
                },
                new Interest
                {
                    Id = 14,
                    Name = "Education",
                    Description = "Learning, teaching, academia",
                    IconName = "school-outline",
                    Keywords = "education,learn,teach,student,school,university,knowledge,academic,course,degree,professor,curriculum"
                },
                new Interest
                {
                    Id = 15,
                    Name = "Politics",
                    Description = "Government, policy, activism",
                    IconName = "podium-outline",
                    Keywords = "politics,government,policy,vote,election,law,democracy,debate,campaign,party,legislation,advocacy"
                }
            );
        }
    }
}
