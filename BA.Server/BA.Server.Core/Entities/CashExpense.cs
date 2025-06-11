using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BA.Server.Core.Entities
{
    /// <summary>
    /// 現金支出實體
    /// 為什麼要有這個實體？
    /// 1. 記錄使用者的每筆現金支出
    /// 2. 支援預算管理和追蹤
    /// 3. 提供支出歷史查詢
    /// </summary>
    public class CashExpense
    {
        /// <summary>
        /// 支出 ID（主鍵）
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        [Required]
        public required string UserId { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// 支出金額
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string Description { get; set; }

        /// <summary>
        /// 支出分類
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Category { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最後更新日期（支援修改支出功能）
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
    }
}