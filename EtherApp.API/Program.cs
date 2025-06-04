using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.Data.Services.Implementations;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// Register services
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHashtagsService, HashtagService>();
builder.Services.AddScoped<IFriendsService, FriendsService>();
builder.Services.AddScoped<IInterestService, InterestService>();
builder.Services.AddScoped<IContentAnalysisService, HuggingFaceContentAnalysisService>();
builder.Services.AddScoped<IPostsService, PostService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"] ?? "https://localhost:5001",
        ValidAudience = builder.Configuration["JWT:ValidAudience"] ?? "EtherApp",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? "YourSecretKeyHereMakeThisAtLeast32CharactersLong"))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("https://localhost:7000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
