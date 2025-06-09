namespace BA.Server.Core.DTOs.Expense
{
    public class ExpenseResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public int? ExpenseId { get; set; }
        public decimal RemainingBudget { get; set; }
    }
}