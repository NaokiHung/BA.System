namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 現金支出詳細資料回應
    /// 為什麼需要詳細資料回應？
    /// 1. 編輯支出時需要完整的支出資訊
    /// 2. 查看支出詳情時的資料傳輸
    /// 3. 與列表回應分離，避免過度取得資料
    /// </summary>
    public class CashExpenseDetailResponse
    {
        /// <summary>
        /// 支出 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 支出金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 支出分類
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 最後更新日期
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 格式化的日期字串 (用於前端顯示)
        /// </summary>
        public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// 格式化的金額字串 (用於前端顯示)
        /// </summary>
        public string FormattedAmount => Amount.ToString("C");
    }
}