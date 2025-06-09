using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    public class SetBudgetRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "預算金額必須大於 0")]
        public decimal Amount { get; set; }

        [Required]
        [Range(2020, 2050, ErrorMessage = "年份必須在 2020-2050 之間")]
        public int Year { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "月份必須在 1-12 之間")]
        public int Month { get; set; }
    }
}