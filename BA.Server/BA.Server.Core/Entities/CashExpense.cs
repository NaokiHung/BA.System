using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.Entities
{
    /// <summary>
    /// 現金支出實體 - 增強版本
    /// 檔案路徑：BA.Server/BA.Server.Core/Entities/CashExpense.cs
    /// 
    /// 新增支援編輯和刪除功能的欄位
    /// </summary>
    public class CashExpense
    {
        /// <summary>
        /// 支出記錄 ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        [Required]
        [StringLength(450)]
        public required string UserId { get; set; }

        /// <summary>
        /// 支出年份
        /// </summary>
        [Required]
        public int Year { get; set; }

        /// <summary>
        /// 支出月份
        /// </summary>
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// 支出金額
        /// </summary>
        [Required]
        [Range(0.01, 9999999.99)]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支出描述
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string Description { get; set; }

        /// <summary>
        /// 支出類別
        /// </summary>
        [StringLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 最後修改日期
        /// 新增：支援編輯功能
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 支付地點
        /// 新增：記錄消費地點
        /// </summary>
        [StringLength(200)]
        public string? PaymentLocation { get; set; }

        /// <summary>
        /// 備註
        /// 新增：額外的備註資訊
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// 收據編號
        /// 新增：便於對帳
        /// </summary>
        [StringLength(100)]
        public string? ReceiptNumber { get; set; }

        /// <summary>
        /// 是否有收據
        /// </summary>
        public bool HasReceipt { get; set; } = false;

        /// <summary>
        /// 支付方式明細
        /// 例如：現金、悠遊卡等
        /// </summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "現金";

        /// <summary>
        /// 是否為緊急支出
        /// 用於支出分析
        /// </summary>
        public bool IsEmergencyExpense { get; set; } = false;

        /// <summary>
        /// 支出標籤
        /// 未來功能：支援標籤分類
        /// </summary>
        [StringLength(200)]
        public string? Tags { get; set; }

        /// <summary>
        /// 關聯的預算 ID
        /// 未來功能：支援多預算管理
        /// </summary>
        public int? RelatedBudgetId { get; set; }
    }
}