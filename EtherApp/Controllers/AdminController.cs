using EtherApp.Data;
using EtherApp.Data.Models;
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

        public AdminController(
            AppDBContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Shows admin dashboard
        public IActionResult Index()
        {
            return View();
        }

        // Lists all flagged posts
        
    }
}