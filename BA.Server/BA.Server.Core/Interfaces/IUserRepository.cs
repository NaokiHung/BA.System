using BA.Server.Core.Entities;

namespace BA.Server.Core.Interfaces
{
    /// <summary>
    /// 使用者 Repository 介面
    /// 為什麼要建立專用介面？
    /// 1. 提供更明確的型別安全
    /// 2. 可以新增特定於使用者的查詢方法
    /// 3. 便於測試時建立 Mock 物件
    /// 4. 符合介面隔離原則
    /// </summary>
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// 根據使用者名稱查詢使用者
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>使用者物件或 null</returns>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// 根據電子郵件查詢使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者物件或 null</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>是否存在</returns>
        Task<bool> IsUsernameExistsAsync(string username);

        /// <summary>
        /// 檢查電子郵件是否已存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        Task<bool> IsEmailExistsAsync(string email);
    }

    /// <summary>
    /// 支出相關的 Repository 介面
    /// </summary>
    public interface IExpenseRepository : IBaseRepository<CashExpense>
    {
        /// <summary>
        /// 根據日期範圍查詢支出
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>支出清單</returns>
        Task<IEnumerable<CashExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 根據類別查詢支出
        /// </summary>
        /// <param name="category">支出類別</param>
        /// <returns>支出清單</returns>
        Task<IEnumerable<CashExpense>> GetByCategoryAsync(string category);

        /// <summary>
        /// 計算指定月份的總支出
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>總支出金額</returns>
        Task<decimal> GetMonthlyTotalAsync(int year, int month);
    }

    /// <summary>
    /// 信用卡支出 Repository 介面
    /// </summary>
    public interface ICreditCardExpenseRepository : IBaseRepository<CreditCardExpense>
    {
        /// <summary>
        /// 根據信用卡查詢支出
        /// </summary>
        /// <param name="cardName">信用卡名稱</param>
        /// <returns>支出清單</returns>
        Task<IEnumerable<CreditCardExpense>> GetByCardAsync(string cardName);

        /// <summary>
        /// 查詢未分期的支出
        /// </summary>
        /// <returns>支出清單</returns>
        Task<IEnumerable<CreditCardExpense>> GetNonInstallmentExpensesAsync();

        /// <summary>
        /// 查詢分期中的支出
        /// </summary>
        /// <returns>支出清單</returns>
        Task<IEnumerable<CreditCardExpense>> GetInstallmentExpensesAsync();
    }

    /// <summary>
    /// 月度預算 Repository 介面
    /// </summary>
    public interface IMonthlyBudgetRepository : IBaseRepository<MonthlyBudget>
    {
        /// <summary>
        /// 根據年月查詢預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>預算物件或 null</returns>
        Task<MonthlyBudget?> GetByYearMonthAsync(int year, int month);

        /// <summary>
        /// 查詢指定年份的所有預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>預算清單</returns>
        Task<IEnumerable<MonthlyBudget>> GetByYearAsync(int year);

        /// <summary>
        /// 檢查指定年月是否已有預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsByYearMonthAsync(int year, int month);
    }
}