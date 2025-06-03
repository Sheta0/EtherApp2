using EtherApp.Data;
using EtherApp.Data.Helpers;
using EtherApp.Data.Hubs;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.Data.Services.Implementations;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Database Configuration
var dbConnectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB upload
});

//Services Configuration
builder.Services.AddScoped<IPostsService, PostService>();
builder.Services.AddScoped<IHashtagsService, HashtagService>();
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendsService, FriendsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IInterestService, InterestService>();

// Add these lines to your service registration
builder.Services.AddHttpClient("HuggingFace", client =>
{
    // Base configuration for the HttpClient
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IContentAnalysisService, HuggingFaceContentAnalysisService>();


// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password Settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
                .AddEntityFrameworkStores<AppDBContext>()
                .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Authentication/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["Auth:Google:ClientId"] ?? "";
        o.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"] ?? "";
        o.CallbackPath = "/signin-google";
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR();


var app = builder.Build();


// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var dbContext = services.GetRequiredService<AppDBContext>();
    var interestService = services.GetRequiredService<IInterestService>();
    var contentAnalysisService = services.GetRequiredService<IContentAnalysisService>();

    await DBInitializer.SeedUsersAndRolesAsync(userManager, roleManager);
    await DBInitializer.SeedAsync(dbContext, interestService);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
