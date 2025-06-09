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
    }
}