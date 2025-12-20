using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QLDACN.Data;
using QLDACN.Models;

namespace QLDACN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QLDACN.Data.RecyclingDbContext _db;

        public HomeController(ILogger<HomeController> logger, QLDACN.Data.RecyclingDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRates()
        {
            var types = await _db.WasteTypes.Where(w => w.Status == "Active").OrderBy(w => w.Category).ThenBy(w => w.Name).ToListAsync();
            return View(types);
        }

        public async Task<IActionResult> Rewards()
        {
            var gifts = await _db.Gifts
                .Where(g => g.Status == "Active" && g.StockQuantity > 0)
                .OrderBy(g => g.Category)
                .ThenBy(g => g.PointsRequired)
                .ToListAsync();
            decimal userPoints = 0m;
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    var userId = int.Parse(userIdStr);
                    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    userPoints = user?.TotalPoints ?? 0m;
                }
            }
            ViewData["UserPoints"] = userPoints;
            return View(gifts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
