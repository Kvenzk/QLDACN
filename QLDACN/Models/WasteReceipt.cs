namespace QLDACN.Models
{
    public class WasteReceipt
    {
        public int ReceiptId { get; set; }
        public int ScheduledId { get; set; }
        public decimal TotalPoints { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }

        public PickupSchedule? Schedule { get; set; }
        public ICollection<ReceiptDetail> Details { get; set; } = new List<ReceiptDetail>();
    }
}
