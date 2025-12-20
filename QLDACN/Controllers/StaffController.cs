using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDACN.Data;
using QLDACN.Models;
using System.Security.Claims;

namespace QLDACN.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly RecyclingDbContext _db;
        public StaffController(RecyclingDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var totalReceipts = await _db.WasteReceipts.CountAsync();
            var totalRedemptions = await _db.GiftRedemptions.CountAsync();
            ViewData["TotalReceipts"] = totalReceipts;
            ViewData["TotalRedemptions"] = totalRedemptions;
            return View();
        }

        public async Task<IActionResult> Schedules(string? status)
        {
            var query = _db.PickupSchedules
                .Include(s => s.User)
                .Include(s => s.WasteReceipts)
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

        public async Task<IActionResult> Receipts()
        {
            var receipts = await _db.WasteReceipts
                .Include(r => r.Schedule)!.ThenInclude(s => s!.User)
                .Include(r => r.Details)!.ThenInclude(d => d.WasteType)
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

        [HttpGet]
        public async Task<IActionResult> RedemptionDetails(int id)
        {
            var redemption = await _db.GiftRedemptions
                .Include(r => r.User)
                .Include(r => r.Staff)
                .Include(r => r.Details)!.ThenInclude(d => d.Gift)
                .FirstOrDefaultAsync(r => r.RedemptionId == id);
            if (redemption == null) return NotFound();
            return View(redemption);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDelivery(int id)
        {
            var redemption = await _db.GiftRedemptions
                .Include(r => r.User)
                .Include(r => r.Staff)
                .FirstOrDefaultAsync(r => r.RedemptionId == id);
            if (redemption == null) return NotFound();
            if (redemption.Status != "Completed")
            {
                if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
                {
                    redemption.StaffId = staffId;
                }
                redemption.Status = "Completed";
                redemption.UpdatedAt = DateTime.UtcNow;

                // Khấu trừ điểm nếu chưa khấu trừ cho đơn này
                if (redemption.UserId > 0)
                {
                    var alreadyRedeemedTx = await _db.PointTransactions.AnyAsync(p =>
                        p.UserId == redemption.UserId &&
                        p.TransactionType == "Redeem" &&
                        p.ReferenceType == "GiftRedemption" &&
                        p.ReferenceId == redemption.RedemptionId);

                    if (!alreadyRedeemedTx)
                    {
                        _db.PointTransactions.Add(new PointTransaction
                        {
                            UserId = redemption.UserId,
                            TransactionType = "Redeem",
                            Points = redemption.TotalPointsSpent,
                            ReferenceId = redemption.RedemptionId,
                            ReferenceType = "GiftRedemption",
                            Description = "Đổi quà - duyệt bởi nhân viên",
                            CreatedAt = DateTime.UtcNow
                        });
                        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == redemption.UserId);
                        if (user != null)
                        {
                            user.TotalPoints -= redemption.TotalPointsSpent;
                        }
                    }
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Deliveries));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSchedule(int id)
        {
            var schedule = await _db.PickupSchedules.FirstOrDefaultAsync(s => s.ScheduledId == id);
            if (schedule == null) return NotFound();
            if (schedule.Status != "Approved")
            {
                schedule.Status = "Approved";
                schedule.UpdateAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Schedules));
        }

        [HttpGet]
        public async Task<IActionResult> CreateReceipt(int id)
        {
            var schedule = await _db.PickupSchedules
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.ScheduledId == id);
            if (schedule == null) return NotFound();
            var wasteTypes = await _db.WasteTypes
                .Where(w => w.Status == "Active")
                .OrderBy(w => w.Name)
                .ToListAsync();
            ViewBag.Schedule = schedule;
            ViewBag.WasteTypes = wasteTypes;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt(int scheduleId, int[] wasteTypeIds, decimal[] quantities, string? note)
        {
            if (wasteTypeIds == null || quantities == null || wasteTypeIds.Length == 0 || wasteTypeIds.Length != quantities.Length)
                return BadRequest("Dữ liệu không hợp lệ");
            var schedule = await _db.PickupSchedules
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.ScheduledId == scheduleId);
            if (schedule == null) return NotFound();

            var wtIds = wasteTypeIds.Distinct().ToArray();
            var wasteTypes = await _db.WasteTypes
                .Where(w => wtIds.Contains(w.WasteTypeId))
                .ToDictionaryAsync(w => w.WasteTypeId);

            decimal totalPoints = 0m;
            for (int i = 0; i < wasteTypeIds.Length; i++)
            {
                var wtId = wasteTypeIds[i];
                var qty = quantities[i];
                if (!wasteTypes.TryGetValue(wtId, out var wt)) return BadRequest("Loại rác không hợp lệ");
                if (qty <= 0) return BadRequest("Số lượng phải lớn hơn 0");
                totalPoints += wt.PointPerUnit * qty;
            }

            var receipt = new WasteReceipt
            {
                ScheduledId = schedule.ScheduledId,
                TotalPoints = totalPoints,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };
            _db.WasteReceipts.Add(receipt);
            await _db.SaveChangesAsync();

            for (int i = 0; i < wasteTypeIds.Length; i++)
            {
                var wtId = wasteTypeIds[i];
                var qty = quantities[i];
                var wt = wasteTypes[wtId];
                _db.ReceiptDetails.Add(new ReceiptDetail
                {
                    ReceiptId = receipt.ReceiptId,
                    WasteTypeId = wt.WasteTypeId,
                    QuantityOfWaste = qty,
                    PointsPerUnit = wt.PointPerUnit,
                    TotalWastePoints = wt.PointPerUnit * qty
                });
            }

            _db.PointTransactions.Add(new PointTransaction
            {
                UserId = schedule.UserId,
                TransactionType = "Issue",
                Points = totalPoints,
                ReferenceId = receipt.ReceiptId,
                ReferenceType = "WasteReceipt",
                Description = $"Ghi nhận rác từ lịch #{schedule.ScheduledId}",
                CreatedAt = DateTime.UtcNow
            });

            if (schedule.User != null)
            {
                schedule.User.TotalPoints += totalPoints;
            }
            schedule.StaffId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId) ? staffId : schedule.StaffId;
            schedule.UpdateAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Receipts));
        }

        [HttpGet]
        public async Task<IActionResult> ReceiptDetails(int id)
        {
            var receipt = await _db.WasteReceipts
                .Include(r => r.Schedule)!.ThenInclude(s => s!.User)
                .Include(r => r.Details)!.ThenInclude(d => d.WasteType)
                .FirstOrDefaultAsync(r => r.ReceiptId == id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }
    }
}
