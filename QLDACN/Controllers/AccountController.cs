using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDACN.Data;
using QLDACN.Models;
using System.Security.Claims;

namespace QLDACN.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecyclingDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public AccountController(RecyclingDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
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

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            role ??= new Role { RoleName = "User", CreatedAt = DateTime.UtcNow };
            if (role.RoleId == 0)
            {
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();
            }

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
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await SignInAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không đúng");
                return View(model);
            }
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không đúng");
                return View(model);
            }

            await SignInAsync(user, model.RememberMe);
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role?.RoleName == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            if (role?.RoleName == "Staff")
            {
                return RedirectToAction("Index", "Staff");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInAsync(User user, bool isPersistent = false)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role?.RoleName ?? "User")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = isPersistent });
        }
    }
}
