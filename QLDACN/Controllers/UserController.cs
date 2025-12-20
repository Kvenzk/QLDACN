using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDACN.Data;
using System.Security.Claims;

namespace QLDACN.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly RecyclingDbContext _db;
        public UserController(RecyclingDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            ViewData["NetPoints"] = user.TotalPoints;

            return View(user);
        }
    }
}
