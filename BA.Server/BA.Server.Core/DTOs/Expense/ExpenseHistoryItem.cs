/// <summary>
    /// 支出歷史記錄 DTO - 簡化版
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/ExpenseHistoryItem.cs
    /// </summary>
    public class ExpenseHistoryItem
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "其他";
        public string Date { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }