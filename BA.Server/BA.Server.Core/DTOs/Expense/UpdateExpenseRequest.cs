using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.DTOs.Expense
{
    /// <summary>
    /// 更新支出請求 - 符合 Controller 使用的名稱
    /// 為什麼要與 Controller 一致？
    /// 1. 避免命名混淆
    /// 2. 保持 API 層的一致性
    /// 3. 便於前端開發者理解
    /// </summary>
    public class UpdateExpenseRequest
    {
        /// <summary>
        /// 支出金額
        /// 驗證規則：必填，大於 0，最多到小數點後 2 位
        /// </summary>
        [Required(ErrorMessage = "支出金額為必填欄位")]
        [Range(0.01, double.MaxValue, ErrorMessage = "支出金額必須大於 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "金額格式不正確，最多只能有兩位小數")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// 驗證規則：必填，長度限制
        /// </summary>
        [Required(ErrorMessage = "支出描述為必填欄位")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "支出描述長度必須在 1 到 200 字元之間")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 支出分類
        /// 驗證規則：必填，只能選擇預定義的分類
        /// </summary>
        [Required(ErrorMessage = "支出分類為必填欄位")]
        [StringLength(50, ErrorMessage = "分類名稱不能超過 50 字元")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 更新原因（可選）
        /// 用於記錄為什麼修改這筆支出
        /// </summary>
        [StringLength(500, ErrorMessage = "更新原因不能超過 500 字元")]
        public string? UpdateReason { get; set; }
    }
}