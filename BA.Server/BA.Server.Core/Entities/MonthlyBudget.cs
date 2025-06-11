using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.Entities
{
    /// <summary>
    /// 月度預算實體 - 更新版本
    /// 檔案路徑：BA.Server/BA.Server.Core/Entities/MonthlyBudget.cs
    /// 
    /// 新增 TotalAmount 屬性以支援新的服務邏輯
    /// </summary>
    public class MonthlyBudget
    {
        /// <summary>
        /// 預算 ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 預算年份
        /// </summary>
        [Required]
        public int Year { get; set; }

        /// <summary>
        /// 預算月份
        /// </summary>
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// 預算總金額
        /// 新增：原始設定的預算總額
        /// </summary>
        [Required]
        [Range(0.01, 9999999.99)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 剩餘金額
        /// </summary>
        [Required]
        [Range(0, 9999999.99)]
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 最後修改日期
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 預算描述
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// 是否為自動預算
        /// 未來功能：自動根據歷史消費設定預算
        /// </summary>
        public bool IsAutoBudget { get; set; } = false;

        /// <summary>
        /// 預算狀態
        /// Active, Paused, Completed
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// 預算類型
        /// Monthly, Weekly, Custom
        /// </summary>
        [StringLength(20)]
        public string BudgetType { get; set; } = "Monthly";

        /// <summary>
        /// 警告閾值（百分比）
        /// 當預算使用率達到此值時發出警告
        /// </summary>
        [Range(0, 100)]
        public decimal WarningThreshold { get; set; } = 80;

        /// <summary>
        /// 是否啟用警告通知
        /// </summary>
        public bool EnableWarningNotification { get; set; } = true;
    }
}