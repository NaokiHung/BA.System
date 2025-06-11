using BA.Server.Core.DTOs.Expense;

namespace BA.Server.Core.Interfaces
{
    /// <summary>
    /// 支出服務介面 - 增強版本
    /// 檔案路徑：BA.Server/BA.Server.Core/Interfaces/IExpenseService.cs
    /// 
    /// 新增信用卡支出、編輯、刪除等功能的介面定義
    /// </summary>
    public interface IExpenseService
    {
        // === 現有方法（保持不變） ===

        /// <summary>
        /// 取得當月預算資訊
        /// </summary>
        Task<MonthlyBudgetResponse> GetCurrentMonthBudgetAsync(string userId);

        /// <summary>
        /// 新增現金支出
        /// </summary>
        Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request);

        /// <summary>
        /// 設定月預算
        /// </summary>
        Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month);

        /// <summary>
        /// 取得支出歷史記錄
        /// </summary>
        Task<IEnumerable<ExpenseHistory>> GetExpenseHistoryAsync(string userId, int year, int month);

        // === 新增方法 ===

        /// <summary>
        /// 新增信用卡支出
        /// 為什麼需要獨立的信用卡支出方法？
        /// 1. 信用卡支出不會影響現金預算餘額
        /// 2. 需要記錄額外的信用卡資訊（如卡片名稱、分期等）
        /// 3. 便於未來擴展信用卡相關功能
        /// </summary>
        Task<ExpenseResponse> AddCreditCardExpenseAsync(string userId, AddCreditCardExpenseRequest request);

        /// <summary>
        /// 更新支出記錄
        /// 為什麼需要更新功能？
        /// 1. 使用者可能輸入錯誤需要修正
        /// 2. 支出類別或描述可能需要調整
        /// 3. 提升系統的實用性和使用者體驗
        /// </summary>
        Task<ExpenseResponse> UpdateExpenseAsync(string userId, int expenseId, UpdateExpenseRequest request);

        /// <summary>
        /// 刪除支出記錄
        /// 為什麼需要刪除功能？
        /// 1. 使用者可能重複記錄或記錄錯誤
        /// 2. 刪除記錄需要同步更新預算餘額（僅現金支出）
        /// 3. 符合 CRUD 完整性要求
        /// </summary>
        Task<ExpenseResponse> DeleteExpenseAsync(string userId, int expenseId);

        /// <summary>
        /// 取得單一支出記錄詳情
        /// 為什麼需要這個方法？
        /// 1. 支援編輯功能時預載入原始資料
        /// 2. 提供支出記錄的詳細檢視
        /// 3. 確保使用者只能存取自己的支出記錄
        /// </summary>
        Task<ExpenseDetailResponse?> GetExpenseDetailAsync(string userId, int expenseId);
        
         /// <summary>
        /// 更新現金支出記錄
        /// </summary>
        Task<ExpenseResponse> UpdateCashExpenseAsync(string userId, int expenseId, UpdateCashExpenseRequest request);

        /// <summary>
        /// 刪除現金支出記錄
        /// </summary>
        Task<ExpenseResponse> DeleteCashExpenseAsync(string userId, int expenseId);

        /// <summary>
        /// 取得單一支出記錄詳情
        /// </summary>
        Task<CashExpenseDetailResponse?> GetCashExpenseDetailAsync(string userId, int expenseId);

        // === 未來可擴展的方法 ===

        /// <summary>
        /// 取得支出統計資料
        /// 未來功能：提供詳細的消費分析
        /// </summary>
        // Task<ExpenseStatisticsResponse> GetExpenseStatisticsAsync(string userId, int year, int month);

        /// <summary>
        /// 取得類別支出統計
        /// 未來功能：按類別分析消費習慣
        /// </summary>
        // Task<IEnumerable<CategoryExpenseResponse>> GetCategoryStatisticsAsync(string userId, int year, int month);

        /// <summary>
        /// 匯出支出記錄
        /// 未來功能：支援 Excel、CSV 格式匯出
        /// </summary>
        // Task<byte[]> ExportExpenseDataAsync(string userId, int year, int month, string format);

        /// <summary>
        /// 批次匯入支出記錄
        /// 未來功能：從檔案批次匯入支出資料
        /// </summary>
        // Task<ExpenseResponse> ImportExpenseDataAsync(string userId, Stream fileStream, string format);
    }
}