namespace BA.Server.Core.DTOs.Expense
{
    public class MonthlyBudgetResponse
    {
        public decimal TotalBudget { get; set; }           // 當月預算總額
        public decimal RemainingCash { get; set; }         // 剩餘現金預算
        public decimal TotalCashExpenses { get; set; }     // 已支出現金總額
        public decimal TotalSubscriptions { get; set; }    // 訂閱服務總額
        public decimal TotalCreditCard { get; set; }       // 信用卡消費總額
        public decimal CombinedCreditTotal { get; set; }   // 信用卡+訂閱總額
        public int Year { get; set; }
        public int Month { get; set; }
        public required string MonthName { get; set; }              // 顯示用的月份名稱
    }
}