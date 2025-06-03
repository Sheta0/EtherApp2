using EtherApp.API.Services;
using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.Data.Services.Implementations;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace EtherApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EtherApp API", Version = "v1" });

                // Configure Swagger to use JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Database Configuration
            var dbConnectionString = builder.Configuration.GetConnectionString("Default");
            builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(dbConnectionString));

            // Add services
            builder.Services.AddScoped<IPostsService, PostService>();
            builder.Services.AddScoped<IHashtagsService, HashtagService>();
            builder.Services.AddScoped<IStoriesService, StoriesService>();
            builder.Services.AddScoped<IFilesService, FilesService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IFriendsService, FriendsService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IContentAnalysisService, ContentAnalysisService>();
            builder.Services.AddScoped<IInterestService, InterestService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

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

            // JWT Authentication setup
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
        }
    }
}
