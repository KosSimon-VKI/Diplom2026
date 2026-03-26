using System.Collections.Generic;

namespace TransferModels.Orders
{
    public class OrderStockConflictResponse
    {
        public string ErrorCode { get; set; } = "STOCK_CONFLICT";
        public string Message { get; set; } = string.Empty;
        public List<StockConflictItem> Items { get; set; } = new List<StockConflictItem>();
    }

    public class StockConflictItem
    {
        public int SemiFinishedId { get; set; }
        public string SemiFinishedName { get; set; } = string.Empty;
        public decimal Required { get; set; }
        public decimal Available { get; set; }
    }
}
