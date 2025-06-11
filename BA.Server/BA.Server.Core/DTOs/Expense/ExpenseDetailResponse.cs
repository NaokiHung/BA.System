namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 支出記錄詳情回應 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/ExpenseDetailResponse.cs
    /// 
    /// 用於編輯功能的資料載入
    /// 提供完整的支出記錄資訊
    /// </summary>
    public class ExpenseDetailResponse
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
        public string? Category { get; set; }

        /// <summary>
        /// 支出類型（現金或信用卡）
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
        /// 建立日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 支出年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 支出月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// 是否可編輯
        /// 通常只允許編輯當月或近期的記錄
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// 是否可刪除
        /// 通常只允許刪除當月的記錄
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// 最後修改日期
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
    }
}