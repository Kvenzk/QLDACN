using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using QLDACN.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QLDACN.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RecyclingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecyclingDbContext>();
    db.Database.Migrate();

    var adminRole = db.Roles.FirstOrDefault(r => r.RoleName == "Admin");
    var userRole = db.Roles.FirstOrDefault(r => r.RoleName == "User");
    if (adminRole == null || userRole == null)
    {
        if (adminRole == null)
        {
            adminRole = new Role { RoleName = "Admin", CreatedAt = DateTime.UtcNow };
            db.Roles.Add(adminRole);
        }
        if (userRole == null)
        {
            userRole = new Role { RoleName = "User", CreatedAt = DateTime.UtcNow };
            db.Roles.Add(userRole);
        }
        db.SaveChanges();
    }

    if (!db.Users.Any(u => u.Username == "admin"))
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            Username = "admin",
            Email = "admin@gmail.com",
            FullName = "Administrator",
            RoleId = adminRole!.RoleId,
            Status = "Active",
            TotalPoints = 0,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
        db.Users.Add(admin);
        db.SaveChanges();
    }

    if (!db.Gifts.Any())
    {
        var gifts = new[]
        {
            new Gift
            {
                Name = "Bình Giữ Nhiệt Eco Life",
                Description = "Giữ nhiệt lên đến 12h, chất liệu thép không gỉ an toàn.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBOyONGmLfrFkEqPzUkIGxPyyWXucocUp1cJNjrbtI1OFRzHaHxJii5B0hhKVRXS5zR8_1QUqDD6m_nJB1N6aOXBYQ0ZrAkCmTmyu4K6V2fbGjRMuRY_Pwq4CZA7Lpd3BEBO6ye6SQ9lrCbDvgBhvRDDNLUhkvXCBZO2K0k2z50F4Mp2P_uqFBR4za2RVZjdJQv-KuDEhITGaO3NIU6cQrUEMdDCcgRm4e7vZiWWFLxhWYfGIrXEtWvOjL1GI8dtv6GXVxs3G_H4w",
                PointsRequired = 850,
                StockQuantity = 50,
                Status = "Active",
                Category = "Vật phẩm",
                CreatedAt = DateTime.UtcNow
            },
            new Gift
            {
                Name = "Voucher Grab 50K",
                Description = "Áp dụng cho mọi chuyến đi GrabCar và GrabBike.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAstYly5ouxoEjw7xNsOK35vbAr1S6niGY_sRp2QQTJF7OcCwIRKBP8l-AhimOAJy3G9z_Wm9EvO0S3eGU6A5rBzJHAocH6g432sZs8Sdsq3wAgSEEE-3VSx_y9uapfsRI9wV8OK-FkW9SOSq2y6KOz9KDY-RsaO02GjMfzkopXP0zZZGGKV_3kZhErYeZ5pk7NgbL51lZ5aiAZMMhPDJUssoCdbOJofkuFjqNZZYiB3dw0-1SgL2k_3fdy3Nwnn9sccO3tSYRQYw",
                PointsRequired = 500,
                StockQuantity = 200,
                Status = "Active",
                Category = "Voucher",
                CreatedAt = DateTime.UtcNow
            },
            new Gift
            {
                Name = "Trồng 1 Cây Xanh",
                Description = "Đóng góp vào quỹ GreenVietnam để trồng rừng.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCUwatM7y_wV533BBV8-yqHLyFFMl_vDZWfRc_5fGHicwmY7RqM7NXcpNssswyt9R0UayRwhPiInrTR6jFHLi2GGsl8dCAGHvdMr6TXCX8hOFZpNZf7DeNbW9TXO4e_Lz_zXQFPDOdpOTLHA7P4re8sXIw3QeEODXbwP9iCvt1z7wq0wspKn7x11irXCtWmYnwmQ_ombuflPFLJhCWr_Z7WO2nVsYifRtp6qDA7HmlYYGHz7UeQl0Ira0-SXyJduaQtyfrG4TZSQQ",
                PointsRequired = 300,
                StockQuantity = 9999,
                Status = "Active",
                Category = "Quyên góp",
                CreatedAt = DateTime.UtcNow
            },
            new Gift
            {
                Name = "Túi Canvas Limited",
                Description = "Phiên bản giới hạn, chất liệu vải tái chế 100%.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuC4D8DxWP8MZZDeJ8WlKRCqyuFwXdzAtsEGLjmDmlafpU0BHazBDFcOMvSnEWzkh8Knw_CFq7QHK2vX3OoWNtVgsGhDlaI2C4PEtEDn_TVKXoXw9nXCRqoh-UhGkya5lDU613DAXhalwQPO0ZFxnX-gQ7G0WPEprOyErQozc1vo2UndkWCK4yj5E2oM3Adai0QuafCsUPu0DoKxYbTXQIMnYNVy2m6j9D_7IXaAUcEf3MWy7nLo-g-5a9WrG3yWuCsm2JYrPB1_3Q",
                PointsRequired = 1200,
                StockQuantity = 20,
                Status = "Active",
                Category = "Vật phẩm",
                CreatedAt = DateTime.UtcNow
            },
            new Gift
            {
                Name = "Spotify Premium 1 Tháng",
                Description = "Nghe nhạc không quảng cáo, chất lượng cao.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBERspSug9aPiN3F3WLaimcRl-SrENCiQRDZgpeYjkAm3HCmX0kdBbMn8pCiQwk3py_vkw6qSWnS90tg0PRIVQ8MDFbmOEYFXN1FYBkgOvihNW1LSUaXTFs0G94lQXuxX0kXv8Vk0bH5151j-ocXYjmmmAuDa9yelCPZF9roohcawGxqYcSx3oP_VXiTHagAlQVOWr9oGIRBbMSAP_RO3i8HpP_v727UlrbZVqpP1LarYG072b_lsfut9go1vk6ptKfvj9vKwdqbw",
                PointsRequired = 450,
                StockQuantity = 100,
                Status = "Active",
                Category = "Voucher",
                CreatedAt = DateTime.UtcNow
            },
            new Gift
            {
                Name = "Set Bàn Chải Tre",
                Description = "Bộ 3 bàn chải tre tự nhiên, lông mềm kháng khuẩn.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBhksxMsyfA8NHKg_gdbaqQ4dbwwllY5_89kFsgn08GpRwSHcHYmIpbyL8v80ajROMQRyHZI9oMtoQ7MyCbGpWI0YBHDQepp-YIdTHyqFkp1p9MrsLjFoqwMPReGbRtcr19m-IeBwckpgaYDwkHGlY77atQ1nju1jDgAZd-EWxC2sZ7bU2ENr9UiR9RKtFSojJVdSAR2C8kMMFgaQKZaSle9azNnkHYDGCq-1iBRyspdyZc0kx_z3OiWCTzdUGCen_lMSBHTVbA8Q",
                PointsRequired = 400,
                StockQuantity = 80,
                Status = "Active",
                Category = "Vật phẩm",
                CreatedAt = DateTime.UtcNow
            }
        };
        db.Gifts.AddRange(gifts);
        db.SaveChanges();
    }
}

app.Run();
