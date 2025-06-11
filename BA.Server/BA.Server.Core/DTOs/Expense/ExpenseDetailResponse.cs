namespace BA.Server.Core.DTOs.Expense
{
    public class ExpenseDetailResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public ExpenseDetail? Data { get; set; }
    }

    public class ExpenseDetail
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public required string Description { get; set; }
        public required string Category { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public required string CreatedDate { get; set; }
        public string? UpdatedDate { get; set; }
    }
}