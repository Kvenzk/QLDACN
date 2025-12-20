namespace QLDACN.Models
{
    public class PickupSchedule
    {
        public int ScheduledId { get; set; }
        public int UserId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? StaffId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public User? User { get; set; }
        public User? Staff { get; set; }
        public ICollection<WasteReceipt> WasteReceipts { get; set; } = new List<WasteReceipt>();
    }
}
