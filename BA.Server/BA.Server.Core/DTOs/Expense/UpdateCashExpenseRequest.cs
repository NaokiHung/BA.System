using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 更新現金支出請求 DTO
    /// 檔案路徑：BA.Server/BA.Server.Core/DTOs/Expense/UpdateCashExpenseRequest.cs
    /// </summary>
    public class UpdateCashExpenseRequest
    {
        [Required(ErrorMessage = "支出金額不能為空")]
        [Range(0.01, 9999999.99, ErrorMessage = "支出金額必須在 0.01 到 9,999,999.99 之間")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "支出描述不能為空")]
        [StringLength(200, ErrorMessage = "支出描述不能超過 200 個字元")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "支出類別不能超過 50 個字元")]
        public string? Category { get; set; }
    }
}