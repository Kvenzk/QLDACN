namespace QLDACN.Models
{
    public class StatisticsSummary
    {
        public int SummaryId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalWasteWeight { get; set; }
        public decimal TotalPointsIssued { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public int TotalUsersActive { get; set; }
        public int TotalReceipts { get; set; }
        public int TotalRedemptions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
