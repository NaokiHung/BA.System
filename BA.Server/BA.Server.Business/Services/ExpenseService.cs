using Microsoft.Extensions.Logging;
using System.Globalization;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;

namespace BA.Server.Business.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IBaseRepository<MonthlyBudget> _budgetRepository;
        private readonly IBaseRepository<CashExpense> _expenseRepository;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(
            IBaseRepository<MonthlyBudget> budgetRepository,
            IBaseRepository<CashExpense> expenseRepository,
            ILogger<ExpenseService> logger)
        {
            _budgetRepository = budgetRepository;
            _expenseRepository = expenseRepository;
            _logger = logger;
        }

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

        public async Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request)
        {
            try
            {
                var currentDate = DateTime.UtcNow; // 使用 UTC 時間
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
                    Category = request.Category ?? "未分類",
                    CreatedDate = currentDate,
                    UpdatedDate = null // 新建記錄時 UpdatedDate 為 null
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

        public async Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month)
        {
            try
            {
                // 步驟1：檢查預算是否已存在
                var existingBudgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var existingBudget = existingBudgets.FirstOrDefault();

                if (existingBudget != null)
                {
                    // 更新現有預算
                    var difference = amount - existingBudget.Amount;
                    existingBudget.Amount = amount;
                    existingBudget.RemainingAmount += difference;
                    existingBudget.UpdatedDate = DateTime.UtcNow;
                    
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
                    // 新增預算
                    var budget = new MonthlyBudget
                    {
                        UserId = userId,
                        Year = year,
                        Month = month,
                        Amount = amount,
                        RemainingAmount = amount,
                        CreatedDate = DateTime.UtcNow
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
                                  Date = e.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                              });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        // 新增缺少的方法實現

        public async Task<ExpenseResponse> AddCreditCardExpenseAsync(string userId, object request)
        {
            // 暫時實現：信用卡支出功能尚未完成
            _logger.LogWarning("信用卡支出功能尚未實現，使用者：{UserId}", userId);
            
            return new ExpenseResponse
            {
                Success = false,
                Message = "信用卡支出功能尚未開放，敬請期待"
            };
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(string userId, int expenseId, UpdateExpenseRequest request)
        {
            try
            {
                // 步驟1：檢查支出記錄是否存在且屬於該使用者
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄"
                    };
                }

                // 步驟2：計算金額差異，更新預算
                var amountDifference = request.Amount - expense.Amount;
                
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                var budget = budgets.FirstOrDefault();

                if (budget != null)
                {
                    budget.RemainingAmount -= amountDifference;
                    budget.UpdatedDate = DateTime.UtcNow;
                    await _budgetRepository.UpdateAsync(budget);
                }

                // 步驟3：更新支出記錄
                expense.Amount = request.Amount;
                expense.Description = request.Description;
                expense.Category = request.Category ?? expense.Category;
                expense.UpdatedDate = DateTime.UtcNow;

                await _expenseRepository.UpdateAsync(expense);

                _logger.LogInformation(
                    "使用者 {UserId} 更新支出記錄 {ExpenseId}，金額變更 {AmountDifference}",
                    userId, expenseId, amountDifference);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄更新成功",
                    ExpenseId = expense.Id,
                    RemainingBudget = budget?.RemainingAmount ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新支出記錄時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "更新支出記錄失敗，請稍後再試"
                };
            }
        }

        public async Task<ExpenseResponse> DeleteExpenseAsync(string userId, int expenseId)
        {
            try
            {
                // 步驟1：檢查支出記錄是否存在且屬於該使用者
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄"
                    };
                }

                // 步驟2：恢復預算金額
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                var budget = budgets.FirstOrDefault();

                if (budget != null)
                {
                    budget.RemainingAmount += expense.Amount;
                    budget.UpdatedDate = DateTime.UtcNow;
                    await _budgetRepository.UpdateAsync(budget);
                }

                // 步驟3：刪除支出記錄
                await _expenseRepository.DeleteAsync(expenseId);

                _logger.LogInformation(
                    "使用者 {UserId} 刪除支出記錄 {ExpenseId}，恢復預算 {Amount}",
                    userId, expenseId, expense.Amount);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄刪除成功",
                    RemainingBudget = budget?.RemainingAmount ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除支出記錄時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "刪除支出記錄失敗，請稍後再試"
                };
            }
        }

        public async Task<ExpenseDetailResponse> GetExpenseDetailAsync(string userId, int expenseId)
        {
            try
            {
                // 步驟1：檢查支出記錄是否存在且屬於該使用者
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return new ExpenseDetailResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄",
                        Data = null
                    };
                }

                // 步驟2：回傳支出詳細資訊
                return new ExpenseDetailResponse
                {
                    Success = true,
                    Message = "取得支出詳細資訊成功",
                    Data = new ExpenseDetail
                    {
                        Id = expense.Id,
                        Amount = expense.Amount,
                        Description = expense.Description,
                        Category = expense.Category,
                        Year = expense.Year,
                        Month = expense.Month,
                        CreatedDate = expense.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedDate = expense.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出詳細資訊時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return new ExpenseDetailResponse
                {
                    Success = false,
                    Message = "取得支出詳細資訊失敗，請稍後再試",
                    Data = null
                };
            }
        }
    }
}