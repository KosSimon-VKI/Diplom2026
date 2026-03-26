namespace TransferModels.Orders
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }
        public decimal TotalCalories { get; set; }

        public int? DiscountId { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}