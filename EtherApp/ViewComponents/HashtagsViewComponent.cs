using EtherApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.ViewComponents
{
    public class HashtagsViewComponent : ViewComponent
    {
        private readonly AppDBContext _context;
        public HashtagsViewComponent(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var oneWeekAgoNow = DateTime.Now.AddDays(-7);

            var top3Hashtags = await _context.Hashtags
                .Where(h => h.DateCreate >= oneWeekAgoNow)
                .OrderByDescending(n => n.Count)
                .Take(3)
                .ToListAsync();

            return View(top3Hashtags);
        }
    }
}
