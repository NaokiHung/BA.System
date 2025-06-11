using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    public class UpdateExpenseRequest
    {
        [Required(ErrorMessage = "金額不能為空")]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "描述不能為空")]
        [MaxLength(200, ErrorMessage = "描述長度不能超過200個字元")]
        public required string Description { get; set; }
        
        [MaxLength(50, ErrorMessage = "分類長度不能超過50個字元")]
        public string? Category { get; set; }
    }
}