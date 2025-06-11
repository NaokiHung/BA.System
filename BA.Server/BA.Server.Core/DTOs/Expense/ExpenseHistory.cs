namespace BA.Server.Core.DTOs.Expense
{
    public class ExpenseHistory
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "其他";
        public string Date { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public string? CardName { get; set; }
        public int? Installments { get; set; }
        public string? MerchantName { get; set; }
        public bool IsOnlineTransaction { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}