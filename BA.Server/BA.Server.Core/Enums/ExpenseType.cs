namespace BA.Server.Core.Enums
{
    /// <summary>
    /// 支出類型枚舉
    /// 檔案路徑：BA.Server/BA.Server.Core/Enums/ExpenseType.cs
    /// 
    /// 為什麼要使用枚舉？
    /// 1. 型別安全，避免字串比較錯誤
    /// 2. 便於程式碼維護和擴展
    /// 3. 支援 IntelliSense 自動完成
    /// 4. 可以輕鬆新增新的支出類型
    /// </summary>
    public enum ExpenseType
    {
        /// <summary>
        /// 現金支出
        /// 會立即影響預算餘額
        /// </summary>
        Cash = 1,

        /// <summary>
        /// 信用卡支出
        /// 不會影響現金預算餘額
        /// </summary>
        CreditCard = 2,

        /// <summary>
        /// 轉帳支出
        /// 未來功能：銀行轉帳記錄
        /// </summary>
        BankTransfer = 3,

        /// <summary>
        /// 數位支付
        /// 未來功能：LINE Pay、Apple Pay 等
        /// </summary>
        DigitalPayment = 4,

        /// <summary>
        /// 訂閱服務
        /// 定期付款項目
        /// </summary>
        Subscription = 5,

        /// <summary>
        /// 投資支出
        /// 未來功能：投資記錄管理
        /// </summary>
        Investment = 6
    }

    /// <summary>
    /// 支出類型擴展方法
    /// </summary>
    public static class ExpenseTypeExtensions
    {
        /// <summary>
        /// 取得支出類型的顯示名稱
        /// </summary>
        /// <param name="expenseType">支出類型</param>
        /// <returns>顯示名稱</returns>
        public static string GetDisplayName(this ExpenseType expenseType)
        {
            return expenseType switch
            {
                ExpenseType.Cash => "現金",
                ExpenseType.CreditCard => "信用卡",
                ExpenseType.BankTransfer => "銀行轉帳",
                ExpenseType.DigitalPayment => "數位支付",
                ExpenseType.Subscription => "訂閱服務",
                ExpenseType.Investment => "投資",
                _ => "未知"
            };
        }

        /// <summary>
        /// 判斷是否影響現金預算
        /// </summary>
        /// <param name="expenseType">支出類型</param>
        /// <returns>是否影響現金預算</returns>
        public static bool AffectsCashBudget(this ExpenseType expenseType)
        {
            return expenseType == ExpenseType.Cash;
        }

        /// <summary>
        /// 取得支出類型的圖示名稱（用於前端顯示）
        /// </summary>
        /// <param name="expenseType">支出類型</param>
        /// <returns>Material Design 圖示名稱</returns>
        public static string GetIconName(this ExpenseType expenseType)
        {
            return expenseType switch
            {
                ExpenseType.Cash => "payments",
                ExpenseType.CreditCard => "credit_card",
                ExpenseType.BankTransfer => "account_balance",
                ExpenseType.DigitalPayment => "contactless",
                ExpenseType.Subscription => "subscriptions",
                ExpenseType.Investment => "trending_up",
                _ => "help"
            };
        }

        /// <summary>
        /// 從字串轉換為支出類型
        /// </summary>
        /// <param name="expenseTypeString">支出類型字串</param>
        /// <returns>支出類型枚舉</returns>
        public static ExpenseType FromString(string expenseTypeString)
        {
            return expenseTypeString?.ToLowerInvariant() switch
            {
                "cash" => ExpenseType.Cash,
                "creditcard" => ExpenseType.CreditCard,
                "banktransfer" => ExpenseType.BankTransfer,
                "digitalpayment" => ExpenseType.DigitalPayment,
                "subscription" => ExpenseType.Subscription,
                "investment" => ExpenseType.Investment,
                _ => ExpenseType.Cash // 預設為現金
            };
        }
    }
}