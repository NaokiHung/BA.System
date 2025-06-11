using Microsoft.Extensions.Logging;
using System.Globalization;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;

namespace BA.Server.Business.Services
{
    /// <summary>
    /// 支出服務實作
    /// 為什麼要實作所有介面方法？
    /// 1. 符合介面契約 (Interface Segregation Principle)
    /// 2. 提供完整的 CRUD 操作
    /// 3. 確保編譯成功並滿足依賴注入的需求
    /// 4. 遵循分層應用程式架構的業務邏輯層責任
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IBaseRepository<MonthlyBudget> _budgetRepository;
        private readonly IBaseRepository<CashExpense> _expenseRepository;
        private readonly ILogger<ExpenseService> _logger;

        /// <summary>
        /// 建構子 - 使用依賴注入
        /// 為什麼要注入這些依賴？
        /// 1. Repository：資料存取層，處理所有資料庫操作
        /// 2. Logger：記錄系統運行狀況，便於除錯和監控
        /// 3. 遵循依賴反轉原則 (Dependency Inversion Principle)
        /// </summary>
        public ExpenseService(
            IBaseRepository<MonthlyBudget> budgetRepository,
            IBaseRepository<CashExpense> expenseRepository,
            ILogger<ExpenseService> logger)
        {
            _budgetRepository = budgetRepository;
            _expenseRepository = expenseRepository;
            _logger = logger;
        }

        #region 預算相關方法

        /// <summary>
        /// 取得當月預算資訊
        /// 流程：查詢預算 → 計算支出 → 組合回應資料
        /// </summary>
        public async Task<MonthlyBudgetResponse> GetCurrentMonthBudgetAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.Now;
                var year = currentDate.Year;
                var month = currentDate.Month;

                // 步驟1：取得當月預算
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var budget = budgets.FirstOrDefault();

                // 步驟2：計算當月已支出金額
                var expenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);
                var totalExpenses = expenses.Sum(e => e.Amount);

                // 步驟3：準備回應資料
                var monthName = new DateTime(year, month, 1).ToString("yyyy年MM月", new CultureInfo("zh-TW"));

                return new MonthlyBudgetResponse
                {
                    TotalBudget = budget?.Amount ?? 0,
                    RemainingCash = budget?.RemainingAmount ?? 0,
                    TotalCashExpenses = totalExpenses,
                    TotalSubscriptions = 0, // 暫時設為0，後續實作訂閱功能時再修改
                    TotalCreditCard = 0,    // 暫時設為0，後續實作信用卡功能時再修改
                    CombinedCreditTotal = 0,
                    Year = year,
                    Month = month,
                    MonthName = monthName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得當月預算資訊時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 設定月預算
        /// 流程：檢查是否存在 → 新增或更新預算 → 記錄日誌
        /// </summary>
        public async Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month)
        {
            try
            {
                // 步驟1：檢查該月預算是否已存在
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var existingBudget = budgets.FirstOrDefault();

                if (existingBudget != null)
                {
                    // 更新現有預算
                    var currentExpenses = await _expenseRepository.FindAsync(
                        e => e.UserId == userId && e.Year == year && e.Month == month);
                    var totalExpenses = currentExpenses.Sum(e => e.Amount);

                    existingBudget.Amount = amount;
                    existingBudget.RemainingAmount = amount - totalExpenses;
                    existingBudget.UpdatedDate = DateTime.Now;

                    await _budgetRepository.UpdateAsync(existingBudget);

                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "預算更新成功",
                        RemainingBudget = existingBudget.RemainingAmount
                    };
                }
                else
                {
                    // 建立新預算
                    var budget = new MonthlyBudget
                    {
                        UserId = userId,
                        Year = year,
                        Month = month,
                        Amount = amount,
                        RemainingAmount = amount,
                        CreatedDate = DateTime.Now
                    };

                    await _budgetRepository.AddAsync(budget);
                    
                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "預算設定成功",
                        RemainingBudget = budget.RemainingAmount
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定月預算時發生錯誤，使用者：{UserId}", userId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "設定預算失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 現金支出 CRUD 操作

        /// <summary>
        /// 新增現金支出
        /// 流程：檢查預算 → 驗證餘額 → 新增支出 → 更新預算
        /// </summary>
        public async Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request)
        {
            try
            {
                var currentDate = DateTime.Now;
                var year = currentDate.Year;
                var month = currentDate.Month;

                // 步驟1：檢查當月預算是否存在
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var budget = budgets.FirstOrDefault();

                if (budget == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "請先設定當月預算"
                    };
                }

                // 步驟2：檢查餘額是否足夠
                if (budget.RemainingAmount < request.Amount)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = $"餘額不足，目前剩餘 {budget.RemainingAmount:C}"
                    };
                }

                // 步驟3：新增支出記錄
                var expense = new CashExpense
                {
                    UserId = userId,
                    Year = year,
                    Month = month,
                    Amount = request.Amount,
                    Description = request.Description,
                    Category = request.Category,
                    CreatedDate = currentDate
                };

                await _expenseRepository.AddAsync(expense);

                // 步驟4：更新預算餘額
                budget.RemainingAmount -= request.Amount;
                budget.UpdatedDate = currentDate;
                await _budgetRepository.UpdateAsync(budget);

                _logger.LogInformation(
                    "使用者 {UserId} 新增支出 {Amount}，剩餘預算 {Remaining}",
                    userId, request.Amount, budget.RemainingAmount);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄新增成功",
                    ExpenseId = expense.Id,
                    RemainingBudget = budget.RemainingAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增現金支出時發生錯誤，使用者：{UserId}", userId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "新增支出失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 取得現金支出詳細資料
        /// 流程：驗證權限 → 查詢支出 → 轉換為回應格式
        /// </summary>
        public async Task<CashExpenseDetailResponse> GetCashExpenseDetailAsync(string userId, int expenseId)
        {
            try
            {
                // 查詢支出記錄，同時驗證是否屬於該使用者
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    throw new UnauthorizedAccessException("找不到指定的支出記錄或您沒有權限存取");
                }

                return new CashExpenseDetailResponse
                {
                    Id = expense.Id,
                    Amount = expense.Amount,
                    Description = expense.Description,
                    Category = expense.Category,
                    CreatedDate = expense.CreatedDate,
                    UpdatedDate = expense.UpdatedDate,
                    Year = expense.Year,
                    Month = expense.Month
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出詳細資料時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                throw;
            }
        }

        /// <summary>
        /// 更新現金支出
        /// 流程：驗證權限 → 計算差額 → 檢查預算 → 更新支出 → 調整預算
        /// </summary>
        public async Task<ExpenseResponse> UpdateCashExpenseAsync(string userId, int expenseId, UpdateCashExpenseRequest request)
        {
            try
            {
                // 步驟1：取得原支出記錄，同時驗證權限
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄或您沒有權限修改"
                    };
                }

                // 步驟2：計算金額差異
                var amountDifference = request.Amount - expense.Amount;

                // 步驟3：如果金額增加，檢查預算是否足夠
                if (amountDifference > 0)
                {
                    var budgets = await _budgetRepository.FindAsync(
                        b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                    var budget = budgets.FirstOrDefault();

                    if (budget == null || budget.RemainingAmount < amountDifference)
                    {
                        return new ExpenseResponse
                        {
                            Success = false,
                            Message = $"餘額不足，無法增加 {amountDifference:C}"
                        };
                    }

                    // 更新預算餘額
                    budget.RemainingAmount -= amountDifference;
                    budget.UpdatedDate = DateTime.Now;
                    await _budgetRepository.UpdateAsync(budget);
                }
                else if (amountDifference < 0)
                {
                    // 金額減少，需要增加預算餘額
                    var budgets = await _budgetRepository.FindAsync(
                        b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                    var budget = budgets.FirstOrDefault();

                    if (budget != null)
                    {
                        budget.RemainingAmount -= amountDifference; // amountDifference 是負數，所以用減法
                        budget.UpdatedDate = DateTime.Now;
                        await _budgetRepository.UpdateAsync(budget);
                    }
                }

                // 步驟4：更新支出記錄
                expense.Amount = request.Amount;
                expense.Description = request.Description;
                expense.Category = request.Category;
                expense.UpdatedDate = DateTime.Now;

                await _expenseRepository.UpdateAsync(expense);

                _logger.LogInformation(
                    "使用者 {UserId} 更新支出 {ExpenseId}，金額差異：{AmountDifference}",
                    userId, expenseId, amountDifference);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄更新成功",
                    ExpenseId = expense.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新現金支出時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "更新支出失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 刪除現金支出
        /// 流程：驗證權限 → 刪除支出 → 回復預算餘額
        /// </summary>
        public async Task<ExpenseResponse> DeleteCashExpenseAsync(string userId, int expenseId)
        {
            try
            {
                // 步驟1：取得支出記錄，同時驗證權限
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄或您沒有權限刪除"
                    };
                }

                // 步驟2：刪除支出記錄
                await _expenseRepository.DeleteAsync(expense);

                // 步驟3：回復預算餘額
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                var budget = budgets.FirstOrDefault();

                if (budget != null)
                {
                    budget.RemainingAmount += expense.Amount;
                    budget.UpdatedDate = DateTime.Now;
                    await _budgetRepository.UpdateAsync(budget);
                }

                _logger.LogInformation(
                    "使用者 {UserId} 刪除支出 {ExpenseId}，回復金額：{Amount}",
                    userId, expenseId, expense.Amount);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄刪除成功，預算已回復"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除現金支出時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "刪除支出失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 歷史記錄查詢

        /// <summary>
        /// 取得支出歷史記錄
        /// 流程：查詢資料 → 排序 → 轉換格式
        /// </summary>
        public async Task<IEnumerable<object>> GetExpenseHistoryAsync(string userId, int year, int month)
        {
            try
            {
                var expenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);

                return expenses.OrderByDescending(e => e.CreatedDate)
                              .Select(e => new
                              {
                                  Id = e.Id,
                                  Amount = e.Amount,
                                  Description = e.Description,
                                  Category = e.Category,
                                  Date = e.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                                  UpdatedDate = e.UpdatedDate?.ToString("yyyy-MM-dd HH:mm")
                              });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        #endregion
    }
}