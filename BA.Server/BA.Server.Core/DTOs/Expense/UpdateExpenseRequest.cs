using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    public class UpdateExpenseRequest
    {
        [Required(ErrorMessage = "支出金額不能為空")]
        [Range(0.01, 9999999.99, ErrorMessage = "支出金額必須在 0.01 到 9,999,999.99 之間")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "支出描述不能為空")]
        [StringLength(200, ErrorMessage = "支出描述不能超過 200 個字元")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "支出類別不能超過 50 個字元")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "支出類型不能為空")]
        public string ExpenseType { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "信用卡名稱不能超過 100 個字元")]
        public string? CardName { get; set; }

        [Range(1, 60, ErrorMessage = "分期期數必須在 1 到 60 之間")]
        public int? Installments { get; set; }

        [StringLength(200, ErrorMessage = "商店名稱不能超過 200 個字元")]
        public string? MerchantName { get; set; }

        public bool? IsOnlineTransaction { get; set; }
    }
}