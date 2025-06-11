/// <summary>
    /// 現金支出詳情回應 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/CashExpenseDetailResponse.cs
    /// </summary>
    public class CashExpenseDetailResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }