namespace QLDACN.Models
{
    public class PointTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}
