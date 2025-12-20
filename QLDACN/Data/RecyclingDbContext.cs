using Microsoft.EntityFrameworkCore;
using QLDACN.Models;

namespace QLDACN.Data
{
    public class RecyclingDbContext : DbContext
    {
        public RecyclingDbContext(DbContextOptions<RecyclingDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<WasteType> WasteTypes => Set<WasteType>();
        public DbSet<PickupSchedule> PickupSchedules => Set<PickupSchedule>();
        public DbSet<WasteReceipt> WasteReceipts => Set<WasteReceipt>();
        public DbSet<ReceiptDetail> ReceiptDetails => Set<ReceiptDetail>();
        public DbSet<Gift> Gifts => Set<Gift>();
        public DbSet<GiftRedemption> GiftRedemptions => Set<GiftRedemption>();
        public DbSet<RedemptionDetail> RedemptionDetails => Set<RedemptionDetail>();
        public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
        public DbSet<StatisticsSummary> StatisticsSummaries => Set<StatisticsSummary>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasKey(x => x.RoleId);
            modelBuilder.Entity<User>().HasKey(x => x.UserId);
            modelBuilder.Entity<RefreshToken>().HasKey(x => x.TokenId);
            modelBuilder.Entity<WasteType>().HasKey(x => x.WasteTypeId);
            modelBuilder.Entity<PickupSchedule>().HasKey(x => x.ScheduledId);
            modelBuilder.Entity<WasteReceipt>().HasKey(x => x.ReceiptId);
            modelBuilder.Entity<Gift>().HasKey(x => x.GiftId);
            modelBuilder.Entity<GiftRedemption>().HasKey(x => x.RedemptionId);
            modelBuilder.Entity<PointTransaction>().HasKey(x => x.TransactionId);
            modelBuilder.Entity<StatisticsSummary>().HasKey(x => x.SummaryId);

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", CreatedAt = new DateTime(2025, 12, 12) },
                new Role { RoleId = 2, RoleName = "User", CreatedAt = new DateTime(2025, 12, 12) },
                new Role { RoleId = 3, RoleName = "Staff", CreatedAt = new DateTime(2025, 12, 12) }
            );

            modelBuilder.Entity<ReceiptDetail>().HasKey(x => new { x.ReceiptId, x.WasteTypeId });
            modelBuilder.Entity<RedemptionDetail>().HasKey(x => new { x.RedemptionId, x.GiftId });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<PickupSchedule>()
                .HasOne(s => s.User)
                .WithMany(u => u.PickupSchedules)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<PickupSchedule>()
                .HasOne(s => s.Staff)
                .WithMany()
                .HasForeignKey(s => s.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WasteReceipt>()
                .HasOne(r => r.Schedule)
                .WithMany(s => s.WasteReceipts)
                .HasForeignKey(r => r.ScheduledId);

            modelBuilder.Entity<ReceiptDetail>()
                .HasOne(d => d.Receipt)
                .WithMany(r => r.Details)
                .HasForeignKey(d => d.ReceiptId);

            modelBuilder.Entity<ReceiptDetail>()
                .HasOne(d => d.WasteType)
                .WithMany(w => w.ReceiptDetails)
                .HasForeignKey(d => d.WasteTypeId);

            modelBuilder.Entity<GiftRedemption>()
                .HasOne(r => r.User)
                .WithMany(u => u.GiftRedemptions)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<GiftRedemption>()
                .HasOne(r => r.Staff)
                .WithMany()
                .HasForeignKey(r => r.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RedemptionDetail>()
                .HasOne(d => d.Redemption)
                .WithMany(r => r.Details)
                .HasForeignKey(d => d.RedemptionId);

            modelBuilder.Entity<RedemptionDetail>()
                .HasOne(d => d.Gift)
                .WithMany(g => g.RedemptionDetails)
                .HasForeignKey(d => d.GiftId);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<PointTransaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.PointTransactions)
                .HasForeignKey(t => t.UserId);
        }
    }
}
