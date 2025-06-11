using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 新增信用卡支出請求 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/AddCreditCardExpenseRequest.cs
    /// 
    /// 為什麼要獨立信用卡支出？
    /// 1. 信用卡支出不會立即影響現金預算
    /// 2. 可能需要記錄信用卡相關資訊
    /// 3. 便於未來擴展信用卡相關功能（如分期、帳單管理）
    /// </summary>
    public class AddCreditCardExpenseRequest
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
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 支出類別
        /// </summary>
        [StringLength(50, ErrorMessage = "支出類別不能超過 50 個字元")]
        public string? Category { get; set; }

        /// <summary>
        /// 信用卡名稱
        /// 例如：中信紅利卡、玉山信用卡等
        /// </summary>
        [StringLength(100, ErrorMessage = "信用卡名稱不能超過 100 個字元")]
        public string? CardName { get; set; }

        /// <summary>
        /// 分期期數
        /// 預設為 1，表示一次付清
        /// </summary>
        [Range(1, 60, ErrorMessage = "分期期數必須在 1 到 60 之間")]
        public int Installments { get; set; } = 1;

        /// <summary>
        /// 是否為線上消費
        /// </summary>
        public bool IsOnlineTransaction { get; set; } = false;

        /// <summary>
        /// 商店名稱
        /// </summary>
        [StringLength(200, ErrorMessage = "商店名稱不能超過 200 個字元")]
        public string? MerchantName { get; set; }
    }
}