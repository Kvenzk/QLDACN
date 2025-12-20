using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDACN.Data;
using QLDACN.Models;
using System.Security.Claims;

namespace QLDACN.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly RecyclingDbContext _db;
        public ActivityController(RecyclingDbContext db) { _db = db; }

        public async Task<IActionResult> Receipts()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var receipts = await _db.WasteReceipts
                .Include(r => r.Schedule)!.ThenInclude(s => s!.User)
                .Where(r => r.Schedule!.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(receipts);
        }

        [HttpGet]
        public async Task<IActionResult> ReceiptDetails(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var receipt = await _db.WasteReceipts
                .Include(r => r.Schedule)!.ThenInclude(s => s!.User)
                .Include(r => r.Details)!.ThenInclude(d => d.WasteType)
                .FirstOrDefaultAsync(r => r.ReceiptId == id && r.Schedule!.UserId == userId);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        public async Task<IActionResult> Redemptions()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var list = await _db.GiftRedemptions
                .Include(d => d.User)
                .Include(d => d.Staff)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> RedemptionDetails(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var redemption = await _db.GiftRedemptions
                .Include(r => r.User)
                .Include(r => r.Staff)
                .Include(r => r.Details)!.ThenInclude(d => d.Gift)
                .FirstOrDefaultAsync(r => r.RedemptionId == id && r.UserId == userId);
            if (redemption == null) return NotFound();
            return View(redemption);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRedemption()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var gifts = await _db.Gifts
                .Where(g => g.Status == "Active" && g.StockQuantity > 0)
                .OrderBy(g => g.Category)
                .ThenBy(g => g.PointsRequired)
                .ToListAsync();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            ViewBag.NetPoints = user?.TotalPoints ?? 0m;
            ViewBag.Gifts = gifts;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRedemption(int[] giftIds, int[] quantities, string? note, string? deliveryAddress)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);

            if (giftIds == null || quantities == null || giftIds.Length == 0 || giftIds.Length != quantities.Length)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn quà và số lượng hợp lệ");
            }

            var giftList = await _db.Gifts
                .Where(g => g.Status == "Active" && g.StockQuantity > 0)
                .OrderBy(g => g.Category)
                .ThenBy(g => g.PointsRequired)
                .ToListAsync();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var netPoints = user?.TotalPoints ?? 0m;
            ViewBag.NetPoints = netPoints;
            ViewBag.Gifts = giftList;

            if (!ModelState.IsValid) return View();

            var selectedGifts = await _db.Gifts.Where(g => giftIds.Contains(g.GiftId)).ToListAsync();
            decimal totalSpent = 0m;
            for (int i = 0; i < giftIds.Length; i++)
            {
                var gid = giftIds[i];
                var qty = Math.Max(0, quantities[i]);
                var gift = selectedGifts.FirstOrDefault(g => g.GiftId == gid);
                if (gift == null || qty <= 0) continue;
                if (gift.StockQuantity < qty)
                {
                    ModelState.AddModelError(string.Empty, $"Quà '{gift.Name}' không đủ tồn kho");
                    return View();
                }
                totalSpent += gift.PointsRequired * qty;
            }

            if (totalSpent <= 0)
            {
                ModelState.AddModelError(string.Empty, "Tổng điểm sử dụng phải lớn hơn 0");
                return View();
            }
            if (netPoints < totalSpent)
            {
                ModelState.AddModelError(string.Empty, "Điểm khả dụng không đủ để đổi quà");
                return View();
            }

            var redemption = new GiftRedemption
            {
                UserId = userId,
                TotalPointsSpent = totalSpent,
                Status = "Pending",
                Note = note,
                DeliveryAddress = deliveryAddress,
                CreatedAt = DateTime.UtcNow
            };
            _db.GiftRedemptions.Add(redemption);
            await _db.SaveChangesAsync();

            for (int i = 0; i < giftIds.Length; i++)
            {
                var gid = giftIds[i];
                var qty = Math.Max(0, quantities[i]);
                var gift = selectedGifts.FirstOrDefault(g => g.GiftId == gid);
                if (gift == null || qty <= 0) continue;
                _db.RedemptionDetails.Add(new RedemptionDetail
                {
                    RedemptionId = redemption.RedemptionId,
                    GiftId = gid,
                    Quantity = qty,
                    PointsSpent = gift.PointsRequired,
                    TotalPoints = gift.PointsRequired * qty
                });
                gift.StockQuantity -= qty;
            }

            _db.PointTransactions.Add(new PointTransaction
            {
                UserId = userId,
                TransactionType = "Redeem",
                Points = totalSpent,
                ReferenceId = redemption.RedemptionId,
                ReferenceType = "GiftRedemption",
                Description = "Đổi quà từ điểm",
                CreatedAt = DateTime.UtcNow
            });
            if (user != null)
            {
                user.TotalPoints -= totalSpent;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Redemptions));
        }

        public async Task<IActionResult> Points()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var userId = int.Parse(userIdStr);
            var tx = await _db.PointTransactions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(tx);
        }
    }
}
