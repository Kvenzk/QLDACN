using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDACN.Data;
using QLDACN.Models;
using System.Security.Claims;

namespace QLDACN.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly RecyclingDbContext _db;
        public ScheduleController(RecyclingDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? status)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var query = _db.PickupSchedules.Where(s => s.UserId == userId);
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => s.Status == "Pending");
                else if (status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => s.Status == "Approved");
            }
            var schedules = await query
                .OrderByDescending(s => s.ScheduledDate)
                .ToListAsync();
            ViewData["StatusFilter"] = string.IsNullOrWhiteSpace(status) ? "all" : status.ToLower();
            return View(schedules);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var dt = DateTime.Now.AddDays(1);
            dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            return View(new PickupSchedule { ScheduledDate = dt });
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("PickupAddress,ScheduledDate,Note")] PickupSchedule input)
        {
            if (string.IsNullOrWhiteSpace(input.PickupAddress))
            {
                ModelState.AddModelError("PickupAddress", "Vui lòng nhập địa chỉ thu gom");
            }
            if (input.ScheduledDate <= DateTime.Now)
            {
                ModelState.AddModelError("ScheduledDate", "Thời gian phải ở tương lai");
            }
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var schedule = new PickupSchedule
            {
                PickupAddress = input.PickupAddress,
                ScheduledDate = input.ScheduledDate,
                Note = input.Note,
                UserId = int.Parse(userIdStr!),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _db.PickupSchedules.Add(schedule);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
