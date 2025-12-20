namespace QLDACN.Models
{
    public class ReceiptDetail
    {
        public int ReceiptId { get; set; }
        public int WasteTypeId { get; set; }
        public decimal QuantityOfWaste { get; set; }
        public decimal PointsPerUnit { get; set; }
        public decimal TotalWastePoints { get; set; }

        public WasteReceipt? Receipt { get; set; }
        public WasteType? WasteType { get; set; }
    }
}
