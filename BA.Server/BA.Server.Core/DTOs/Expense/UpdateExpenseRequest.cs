using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 更新支出記錄請求 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/UpdateExpenseRequest.cs
    /// 
    /// 為什麼需要更新功能？
    /// 1. 使用者可能輸入錯誤需要修正
    /// 2. 支出類別或描述可能需要調整
    /// 3. 提升系統的實用性和使用者體驗
    /// </summary>
    public class UpdateExpenseRequest
    {
        /// <summary>
        /// 支出金額
        /// </summary>
        [Required(ErrorMessage = "支出金額不能為空")]
        [Range(0.01, 9999999.99, ErrorMessage = "支出金額必須在 0.01 到 9,999,999.99 之間")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// </summary>
        [Required(ErrorMessage = "支出描述不能為空")]
        [StringLength(200, ErrorMessage = "支出描述不能超過 200 個字元")]
        public required string Description { get; set; }

        /// <summary>
        /// 支出類別
        /// </summary>
        [StringLength(50, ErrorMessage = "支出類別不能超過 50 個字元")]
        public string? Category { get; set; }

        /// <summary>
        /// 支出類型
        /// 確保不會意外改變支出類型（現金 vs 信用卡）
        /// </summary>
        [Required(ErrorMessage = "支出類型不能為空")]
        public required string ExpenseType { get; set; }

        /// <summary>
        /// 信用卡名稱（僅用於信用卡支出）
        /// </summary>
        [StringLength(100, ErrorMessage = "信用卡名稱不能超過 100 個字元")]
        public string? CardName { get; set; }

        /// <summary>
        /// 分期期數（僅用於信用卡支出）
        /// </summary>
        [Range(1, 60, ErrorMessage = "分期期數必須在 1 到 60 之間")]
        public int? Installments { get; set; }

        /// <summary>
        /// 商店名稱
        /// </summary>
        [StringLength(200, ErrorMessage = "商店名稱不能超過 200 個字元")]
        public string? MerchantName { get; set; }

        /// <summary>
        /// 是否為線上消費（僅用於信用卡支出）
        /// </summary>
        public bool? IsOnlineTransaction { get; set; }
    }
}