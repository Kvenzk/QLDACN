namespace QLDACN.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int RoleId { get; set; }
        public decimal TotalPoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Role? Role { get; set; }
        public ICollection<PickupSchedule> PickupSchedules { get; set; } = new List<PickupSchedule>();
        public ICollection<GiftRedemption> GiftRedemptions { get; set; } = new List<GiftRedemption>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    }
}
