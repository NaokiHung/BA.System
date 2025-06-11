namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 月度預算回應 DTO - 增強版本
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/MonthlyBudgetResponse.cs
    /// 
    /// 新增信用卡支出統計和更詳細的預算資訊
    /// </summary>
    public class MonthlyBudgetResponse
    {
        /// <summary>
        /// 當月預算總額
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// 剩餘現金預算
        /// 只有現金支出會影響此數值
        /// </summary>
        public decimal RemainingCash { get; set; }

        /// <summary>
        /// 已支出現金總額
        /// </summary>
        public decimal TotalCashExpenses { get; set; }

        /// <summary>
        /// 訂閱服務總額
        /// 未來功能：定期付款/訂閱管理
        /// </summary>
        public decimal TotalSubscriptions { get; set; }

        /// <summary>
        /// 信用卡消費總額
        /// 新增：統計當月信用卡支出
        /// </summary>
        public decimal TotalCreditCard { get; set; }

        /// <summary>
        /// 信用卡+訂閱總額
        /// 便於前端顯示統計
        /// </summary>
        public decimal CombinedCreditTotal { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 顯示用的月份名稱
        /// 例如：一月、二月等
        /// </summary>
        public string MonthName { get; set; } = string.Empty;

        /// <summary>
        /// 預算使用率（百分比）
        /// 基於現金支出計算
        /// </summary>
        public decimal BudgetUtilizationPercentage { get; set; }

        /// <summary>
        /// 當月剩餘天數
        /// </summary>
        public int RemainingDaysInMonth { get; set; }

        /// <summary>
        /// 平均每日可用預算
        /// 剩餘預算 ÷ 剩餘天數
        /// </summary>
        public decimal AverageDailyBudget { get; set; }

        /// <summary>
        /// 信用卡支出明細
        /// 按信用卡分組的統計
        /// </summary>
        public List<CreditCardSummary> CreditCardSummaries { get; set; } = new List<CreditCardSummary>();

        /// <summary>
        /// 預算狀態
        /// "Healthy", "Warning", "Exceeded"
        /// </summary>
        public string BudgetStatus { get; set; } = "Healthy";
    }

    /// <summary>
    /// 信用卡消費摘要
    /// </summary>
    public class CreditCardSummary
    {
        /// <summary>
        /// 信用卡名稱
        /// </summary>
        public required string CardName { get; set; }

        /// <summary>
        /// 該卡當月消費總額
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 交易筆數
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// 分期交易總額
        /// </summary>
        public decimal InstallmentAmount { get; set; }

        /// <summary>
        /// 線上消費總額
        /// </summary>
        public decimal OnlineAmount { get; set; }
    }
}