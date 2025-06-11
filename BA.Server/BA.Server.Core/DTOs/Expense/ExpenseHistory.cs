namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 支出歷史記錄回應 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/ExpenseHistory.cs
    /// 
    /// 用於支出記錄列表顯示
    /// 包含支出類型和操作權限資訊
    /// </summary>
    public class ExpenseHistory
    {
        /// <summary>
        /// 支出記錄 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 支出金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// 支出類別
        /// </summary>
        public string Category { get; set; } = "其他";

        /// <summary>
        /// 支出日期
        /// 格式化為 ISO 8601 字串格式
        /// </summary>
        public required string Date { get; set; }

        /// <summary>
        /// 支出類型
        /// "Cash" 或 "CreditCard"
        /// </summary>
        public required string ExpenseType { get; set; }

        /// <summary>
        /// 信用卡名稱（僅信用卡支出）
        /// </summary>
        public string? CardName { get; set; }

        /// <summary>
        /// 分期期數（僅信用卡支出）
        /// </summary>
        public int? Installments { get; set; }

        /// <summary>
        /// 商店名稱
        /// </summary>
        public string? MerchantName { get; set; }

        /// <summary>
        /// 是否為線上消費
        /// </summary>
        public bool IsOnlineTransaction { get; set; }

        /// <summary>
        /// 是否可編輯
        /// 根據支出日期和系統設定決定
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// 是否可刪除
        /// 根據支出日期和系統設定決定
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// 支出年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 支出月份
        /// </summary>
        public int Month { get; set; }
    }
}