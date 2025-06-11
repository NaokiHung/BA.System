using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BA.Server.Core.Entities
{
    /// <summary>
    /// 月預算實體
    /// 為什麼要有這個實體？
    /// 1. 管理使用者每月的預算設定
    /// 2. 追蹤剩餘預算
    /// 3. 支援預算控制功能
    /// </summary>
    public class MonthlyBudget
    {
        /// <summary>
        /// 使用者 ID（複合主鍵的一部分）
        /// </summary>
        [Required]
        public required string UserId { get; set; }
        
        /// <summary>
        /// 年份（複合主鍵的一部分）
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// 月份（複合主鍵的一部分）
        /// </summary>
        [Range(1, 12)]
        public int Month { get; set; }
        
        /// <summary>
        /// 預算金額
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 剩餘預算金額
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }
        
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 最後更新日期
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
    }
}