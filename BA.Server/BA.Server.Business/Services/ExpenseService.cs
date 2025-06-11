using Microsoft.Extensions.Logging;
using System.Globalization;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;

namespace BA.Server.Business.Services
{
    /// <summary>
    /// 支出服務實作 - 根據現有程式碼修正版本
    /// 檔案路徑：BA.Server/BA.Server.Business/Services/ExpenseService.cs
    /// </summary>
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

        // === 現有方法（保持原樣） ===

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
                    TotalSubscriptions = 0,
                    TotalCreditCard = 0,
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
                                  Date = e.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                                  CanEdit = CanEditExpense(e.CreatedDate),
                                  CanDelete = CanDeleteExpense(e.CreatedDate)
                              });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        // === 新增方法 ===

        public async Task<ExpenseResponse> UpdateCashExpenseAsync(string userId, int expenseId, UpdateCashExpenseRequest request)
        {
            try
            {
                // 找到支出記錄
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

                // 檢查是否可編輯
                if (!CanEditExpense(expense.CreatedDate))
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "該支出記錄已超過可編輯期限"
                    };
                }

                // 計算金額差異，需要更新預算
                var amountDifference = request.Amount - expense.Amount;

                // 如果金額有變化，需要檢查並更新預算
                if (amountDifference != 0)
                {
                    var budgets = await _budgetRepository.FindAsync(
                        b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                    var budget = budgets.FirstOrDefault();

                    if (budget != null)
                    {
                        // 檢查餘額是否足夠（如果是增加支出）
                        if (amountDifference > 0 && budget.RemainingAmount < amountDifference)
                        {
                            return new ExpenseResponse
                            {
                                Success = false,
                                Message = $"餘額不足，目前剩餘 {budget.RemainingAmount:C}"
                            };
                        }

                        // 更新預算餘額
                        budget.RemainingAmount -= amountDifference;
                        budget.UpdatedDate = DateTime.Now;
                        await _budgetRepository.UpdateAsync(budget);
                    }
                }

                // 更新支出記錄
                expense.Amount = request.Amount;
                expense.Description = request.Description;
                expense.Category = request.Category ?? "其他";

                await _expenseRepository.UpdateAsync(expense);

                _logger.LogInformation(
                    "使用者 {UserId} 更新支出 {ExpenseId}，金額變化：{AmountDifference}",
                    userId, expenseId, amountDifference);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄更新成功",
                    ExpenseId = expense.Id,
                    RemainingBudget = 0 // 前端會重新載入預算資訊
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

        public async Task<ExpenseResponse> DeleteCashExpenseAsync(string userId, int expenseId)
        {
            try
            {
                // 找到支出記錄
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

                // 檢查是否可刪除
                if (!CanDeleteExpense(expense.CreatedDate))
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "該支出記錄已超過可刪除期限"
                    };
                }

                // 刪除支出記錄前，需要退還金額到預算
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == expense.Year && b.Month == expense.Month);
                var budget = budgets.FirstOrDefault();

                if (budget != null)
                {
                    budget.RemainingAmount += expense.Amount;
                    budget.UpdatedDate = DateTime.Now;
                    await _budgetRepository.UpdateAsync(budget);
                }

                // 刪除支出記錄 - 使用 ID
                await _expenseRepository.DeleteAsync(expense.Id);

                _logger.LogInformation(
                    "使用者 {UserId} 刪除支出 {ExpenseId}，退還金額：{Amount}",
                    userId, expense.Id, expense.Amount);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄已刪除，金額已退還至預算",
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

        public async Task<CashExpenseDetailResponse?> GetCashExpenseDetailAsync(string userId, int expenseId)
        {
            try
            {
                var expenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var expense = expenses.FirstOrDefault();

                if (expense == null)
                {
                    return null;
                }

                return new CashExpenseDetailResponse
                {
                    Id = expense.Id,
                    Amount = expense.Amount,
                    Description = expense.Description,
                    Category = expense.Category ?? "其他",
                    CreatedDate = expense.CreatedDate,
                    Year = expense.Year,
                    Month = expense.Month,
                    UserId = expense.UserId,
                    CanEdit = CanEditExpense(expense.CreatedDate),
                    CanDelete = CanDeleteExpense(expense.CreatedDate)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出記錄詳情時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return null;
            }
        }

        // === 私有輔助方法 ===

        private bool CanEditExpense(DateTime expenseDate)
        {
            // 只允許編輯一個月內的記錄
            return DateTime.Now.Subtract(expenseDate).TotalDays <= 30;
        }

        private bool CanDeleteExpense(DateTime expenseDate)
        {
            // 只允許刪除當月的記錄
            var now = DateTime.Now;
            return expenseDate.Year == now.Year && expenseDate.Month == now.Month;
        }
    }
}