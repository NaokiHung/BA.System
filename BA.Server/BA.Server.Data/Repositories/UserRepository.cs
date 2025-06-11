using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Contexts;

namespace BA.Server.Data.Repositories
{
    /// <summary>
    /// 使用者 Repository 實作
    /// 繼承 BaseRepository 並實作 IUserRepository
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        /// <summary>
        /// 建構子：注入 UserDbContext
        /// </summary>
        /// <param name="context">使用者資料庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public UserRepository(UserDbContext context, ILogger<UserRepository> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// 根據使用者名稱查詢使用者
        /// 流程說明：
        /// 1. 使用 LINQ 查詢條件
        /// 2. 使用 AsNoTracking 提升效能
        /// 3. 使用 FirstOrDefaultAsync 取得單一結果
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>使用者物件或 null</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                _logger.LogDebug("正在根據使用者名稱查詢使用者：{Username}", username);
                
                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據使用者名稱查詢使用者時發生錯誤：{Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 根據電子郵件查詢使用者
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>使用者物件或 null</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogDebug("正在根據電子郵件查詢使用者：{Email}", email);
                
                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據電子郵件查詢使用者時發生錯誤：{Email}", email);
                throw;
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// 使用 AnyAsync 比 FirstOrDefaultAsync 更有效率
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                _logger.LogDebug("正在檢查使用者名稱是否存在：{Username}", username);
                
                return await _dbSet.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查使用者名稱是否存在時發生錯誤：{Username}", username);
                throw;
            }
        }

        /// <summary>
        /// 檢查電子郵件是否已存在
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                _logger.LogDebug("正在檢查電子郵件是否存在：{Email}", email);
                
                return await _dbSet.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查電子郵件是否存在時發生錯誤：{Email}", email);
                throw;
            }
        }
    }

    /// <summary>
    /// 現金支出 Repository 實作
    /// </summary>
    public class ExpenseRepository : BaseRepository<CashExpense>, IExpenseRepository
    {
        public ExpenseRepository(ExpenseDbContext context, ILogger<ExpenseRepository> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// 根據日期範圍查詢支出
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>支出清單</returns>
        public async Task<IEnumerable<CashExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogDebug("正在查詢日期範圍內的支出：{StartDate} 到 {EndDate}", startDate, endDate);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.Date >= startDate && e.Date <= endDate)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢日期範圍內的支出時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 根據類別查詢支出
        /// </summary>
        /// <param name="category">支出類別</param>
        /// <returns>支出清單</returns>
        public async Task<IEnumerable<CashExpense>> GetByCategoryAsync(string category)
        {
            try
            {
                _logger.LogDebug("正在根據類別查詢支出：{Category}", category);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.Category == category)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據類別查詢支出時發生錯誤：{Category}", category);
                throw;
            }
        }

        /// <summary>
        /// 計算指定月份的總支出
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>總支出金額</returns>
        public async Task<decimal> GetMonthlyTotalAsync(int year, int month)
        {
            try
            {
                _logger.LogDebug("正在計算月份總支出：{Year}-{Month}", year, month);
                
                return await _dbSet
                    .Where(e => e.Date.Year == year && e.Date.Month == month)
                    .SumAsync(e => e.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算月份總支出時發生錯誤：{Year}-{Month}", year, month);
                throw;
            }
        }
    }

    /// <summary>
    /// 信用卡支出 Repository 實作
    /// </summary>
    public class CreditCardExpenseRepository : BaseRepository<CreditCardExpense>, ICreditCardExpenseRepository
    {
        public CreditCardExpenseRepository(ExpenseDbContext context, ILogger<CreditCardExpenseRepository> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// 根據信用卡查詢支出
        /// </summary>
        /// <param name="cardName">信用卡名稱</param>
        /// <returns>支出清單</returns>
        public async Task<IEnumerable<CreditCardExpense>> GetByCardAsync(string cardName)
        {
            try
            {
                _logger.LogDebug("正在根據信用卡查詢支出：{CardName}", cardName);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.CardName == cardName)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據信用卡查詢支出時發生錯誤：{CardName}", cardName);
                throw;
            }
        }

        /// <summary>
        /// 查詢未分期的支出
        /// </summary>
        /// <returns>支出清單</returns>
        public async Task<IEnumerable<CreditCardExpense>> GetNonInstallmentExpensesAsync()
        {
            try
            {
                _logger.LogDebug("正在查詢未分期的支出");
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.InstallmentMonths <= 1)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢未分期的支出時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 查詢分期中的支出
        /// </summary>
        /// <returns>支出清單</returns>
        public async Task<IEnumerable<CreditCardExpense>> GetInstallmentExpensesAsync()
        {
            try
            {
                _logger.LogDebug("正在查詢分期中的支出");
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.InstallmentMonths > 1)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢分期中的支出時發生錯誤");
                throw;
            }
        }
    }

    /// <summary>
    /// 月度預算 Repository 實作
    /// </summary>
    public class MonthlyBudgetRepository : BaseRepository<MonthlyBudget>, IMonthlyBudgetRepository
    {
        public MonthlyBudgetRepository(ExpenseDbContext context, ILogger<MonthlyBudgetRepository> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// 根據年月查詢預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>預算物件或 null</returns>
        public async Task<MonthlyBudget?> GetByYearMonthAsync(int year, int month)
        {
            try
            {
                _logger.LogDebug("正在查詢年月預算：{Year}-{Month}", year, month);
                
                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Year == year && b.Month == month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢年月預算時發生錯誤：{Year}-{Month}", year, month);
                throw;
            }
        }

        /// <summary>
        /// 查詢指定年份的所有預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>預算清單</returns>
        public async Task<IEnumerable<MonthlyBudget>> GetByYearAsync(int year)
        {
            try
            {
                _logger.LogDebug("正在查詢年度預算：{Year}", year);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(b => b.Year == year)
                    .OrderBy(b => b.Month)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢年度預算時發生錯誤：{Year}", year);
                throw;
            }
        }

        /// <summary>
        /// 檢查指定年月是否已有預算
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsByYearMonthAsync(int year, int month)
        {
            try
            {
                _logger.LogDebug("正在檢查年月預算是否存在：{Year}-{Month}", year, month);
                
                return await _dbSet.AnyAsync(b => b.Year == year && b.Month == month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查年月預算是否存在時發生錯誤：{Year}-{Month}", year, month);
                throw;
            }
        }
    }
}