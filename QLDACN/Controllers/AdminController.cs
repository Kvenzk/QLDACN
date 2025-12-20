using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QLDACN.Data;
using QLDACN.Models;

namespace QLDACN.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RecyclingDbContext _db;
        public AdminController(RecyclingDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalReceipts = await _db.WasteReceipts.CountAsync();
            var totalRedemptions = await _db.GiftRedemptions.CountAsync();
            var totalPointsIssued = await _db.PointTransactions.Where(p => p.TransactionType == "Issue").SumAsync(p => (decimal?)p.Points) ?? 0m;
            var totalPointsRedeemed = await _db.PointTransactions.Where(p => p.TransactionType == "Redeem").SumAsync(p => (decimal?)p.Points) ?? 0m;

            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalReceipts"] = totalReceipts;
            ViewData["TotalRedemptions"] = totalRedemptions;
            ViewData["TotalPointsIssued"] = totalPointsIssued;
            ViewData["TotalPointsRedeemed"] = totalPointsRedeemed;
            return View();
        }

        public async Task<IActionResult> Schedules(string? status)
        {
            var query = _db.PickupSchedules
                .Include(s => s.User)
                .OrderByDescending(s => s.ScheduledDate)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => s.Status == "Pending");
                else if (status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => s.Status == "Approved");
            }
            var list = await query.ToListAsync();
            ViewData["StatusFilter"] = string.IsNullOrWhiteSpace(status) ? "all" : status.ToLower();
            return View(list);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _db.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "User")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Staff()
        {
            var staff = await _db.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Staff")
                .OrderBy(u => u.FullName ?? u.Username)
                .ToListAsync();
            return View(staff);
        }

        [HttpGet]
        public IActionResult StaffCreate()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> StaffCreate(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập đã tồn tại");
                return View(model);
            }
            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(string.Empty, "Email đã được sử dụng");
                return View(model);
            }

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff");
            role ??= new Role { RoleName = "Staff", CreatedAt = DateTime.UtcNow };
            if (role.RoleId == 0)
            {
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                RoleId = role.RoleId,
                Status = "Active",
                TotalPoints = 0,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, model.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Staff");
        }

        public async Task<IActionResult> Receipts()
        {
            var receipts = await _db.WasteReceipts
                .Include(r => r.Schedule)!.ThenInclude(s => s!.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(receipts);
        }

        public async Task<IActionResult> Deliveries()
        {
            var deliveries = await _db.GiftRedemptions
                .Include(d => d.User)
                .Include(d => d.Staff)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(deliveries);
        }

    }
}
