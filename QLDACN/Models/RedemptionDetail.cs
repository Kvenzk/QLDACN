namespace QLDACN.Models
{
    public class RedemptionDetail
    {
        public int RedemptionId { get; set; }
        public int GiftId { get; set; }
        public int Quantity { get; set; }
        public decimal PointsSpent { get; set; }
        public decimal TotalPoints { get; set; }

        public GiftRedemption? Redemption { get; set; }
        public Gift? Gift { get; set; }
    }
}
