namespace QLDACN.Models
{
    public class GiftRedemption
    {
        public int RedemptionId { get; set; }
        public int UserId { get; set; }
        public decimal TotalPointsSpent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? DeliveryNote { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? StaffId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }
        public User? Staff { get; set; }
        public ICollection<RedemptionDetail> Details { get; set; } = new List<RedemptionDetail>();
    }
}
