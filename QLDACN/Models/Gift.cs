namespace QLDACN.Models
{
    public class Gift
    {
        public int GiftId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal PointsRequired { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<RedemptionDetail> RedemptionDetails { get; set; } = new List<RedemptionDetail>();
    }
}
