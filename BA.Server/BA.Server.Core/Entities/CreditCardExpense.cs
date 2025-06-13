using System.ComponentModel.DataAnnotations;

namespace BA.Server.Core.Entities
{
    /// <summary>
    /// 信用卡支出實體
    /// 檔案路徑：BA.Server/BA.Server.Core/Entities/CreditCardExpense.cs
    /// 
    /// 為什麼要獨立信用卡支出實體？
    /// 1. 信用卡支出有特殊屬性（如卡片名稱、分期等）
    /// 2. 不影響現金預算計算
    /// 3. 便於未來擴展信用卡管理功能
    /// 4. 符合領域模型分離原則
    /// </summary>
    public class CreditCardExpense
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
        /// 信用卡名稱
        /// 例如：中信紅利卡、玉山信用卡等
        /// </summary>
        [StringLength(100)]
        public string? CardName { get; set; }

        /// <summary>
        /// 分期期數
        /// 預設為 1，表示一次付清
        /// </summary>
        [Range(1, 60)]
        public int Installments { get; set; } = 1;

        /// <summary>
        /// 是否為線上消費
        /// </summary>
        public bool IsOnlineTransaction { get; set; } = false;

        /// <summary>
        /// 商店名稱
        /// </summary>
        [StringLength(200)]
        public string? MerchantName { get; set; }

        /// <summary>
        /// 交易授權碼
        /// 未來功能：記錄銀行交易授權碼
        /// </summary>
        [StringLength(50)]
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// 信用卡末四碼
        /// 安全考量：只記錄末四碼
        /// </summary>
        [StringLength(4)]
        public string? CardLastFourDigits { get; set; }

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
        /// 是否已入帳
        /// 用於追蹤信用卡帳單狀態
        /// </summary>
        public bool IsBilled { get; set; } = false;

        /// <summary>
        /// 入帳日期
        /// </summary>
        public DateTime? BilledDate { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// 是否為定期付款
        /// 未來功能：訂閱服務追蹤
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// 原始交易金額（外幣）
        /// 未來功能：支援外幣交易
        /// </summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// 原始交易幣別
        /// </summary>
        [StringLength(3)]
        public string? OriginalCurrency { get; set; }

        /// <summary>
        /// 匯率
        /// </summary>
        public decimal? ExchangeRate { get; set; }
    }
}