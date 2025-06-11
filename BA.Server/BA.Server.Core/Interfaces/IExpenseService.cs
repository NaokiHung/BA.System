using BA.Server.Core.DTOs.Expense;

namespace BA.Server.Core.Interfaces
{
    /// <summary>
    /// 支出服務介面
    /// 為什麼要定義介面？
    /// 1. 遵循依賴反轉原則（DIP）
    /// 2. 便於單元測試時進行 Mock
    /// 3. 提供清楚的契約定義
    /// 4. 支援未來的多重實作（例如不同的支出計算邏輯）
    /// </summary>
    public interface IExpenseService
    {
        // 預算相關方法
        /// <summary>
        /// 取得當月預算資訊
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>當月預算回應</returns>
        Task<MonthlyBudgetResponse> GetCurrentMonthBudgetAsync(string userId);

        /// <summary>
        /// 設定月預算
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="amount">預算金額</param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>支出操作回應</returns>
        Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month);

        // 現金支出相關方法
        /// <summary>
        /// 新增現金支出
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="request">新增支出請求</param>
        /// <returns>支出操作回應</returns>
        Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request);

        /// <summary>
        /// 取得現金支出詳細資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="expenseId">支出 ID</param>
        /// <returns>支出詳細資料</returns>
        Task<CashExpenseDetailResponse> GetCashExpenseDetailAsync(string userId, int expenseId);

        /// <summary>
        /// 更新現金支出
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="expenseId">支出 ID</param>
        /// <param name="request">更新支出請求</param>
        /// <returns>支出操作回應</returns>
        Task<ExpenseResponse> UpdateCashExpenseAsync(string userId, int expenseId, UpdateCashExpenseRequest request);

        /// <summary>
        /// 刪除現金支出
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="expenseId">支出 ID</param>
        /// <returns>支出操作回應</returns>
        Task<ExpenseResponse> DeleteCashExpenseAsync(string userId, int expenseId);

        // 歷史記錄相關方法
        /// <summary>
        /// 取得支出歷史記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>支出歷史記錄</returns>
        Task<IEnumerable<object>> GetExpenseHistoryAsync(string userId, int year, int month);
    }
}