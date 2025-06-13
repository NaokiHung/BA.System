using Microsoft.Extensions.Logging;
using System.Globalization;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Entities;
using BA.Server.Core.Enums;
using BA.Server.Core.Interfaces;

namespace BA.Server.Business.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IBaseRepository<MonthlyBudget> _budgetRepository;
        private readonly IBaseRepository<CashExpense> _expenseRepository;
        private readonly IBaseRepository<CreditCardExpense> _creditCardRepository;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(
            IBaseRepository<MonthlyBudget> budgetRepository,
            IBaseRepository<CashExpense> expenseRepository,
            IBaseRepository<CreditCardExpense> creditCardRepository,
            ILogger<ExpenseService> logger)
        {
            _budgetRepository = budgetRepository;
            _expenseRepository = expenseRepository;
            _creditCardRepository = creditCardRepository;
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

                // 步驟2：計算當月現金支出金額
                var expenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);
                var totalCashExpenses = expenses.Sum(e => e.Amount);

                // 步驟3：計算當月信用卡支出金額
                var creditCardExpenses = await _creditCardRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);
                var totalCreditCardExpenses = creditCardExpenses.Sum(e => e.Amount);

                // 步驟4：準備回應資料
                var monthName = new DateTime(year, month, 1).ToString("yyyy年MM月", new CultureInfo("zh-TW"));

                return new MonthlyBudgetResponse
                {
                    TotalBudget = budget?.Amount ?? 0,
                    RemainingCash = budget?.RemainingAmount ?? 0,
                    TotalCashExpenses = totalCashExpenses,
                    TotalSubscriptions = 0, // 暫時設為0，後續實作訂閱功能時再修改
                    TotalCreditCard = totalCreditCardExpenses,
                    CombinedCreditTotal = totalCreditCardExpenses, // 目前等於信用卡支出，訂閱功能實作後會調整
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
                // 取得現金支出
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);

                // 建立統一的支出記錄列表
                var allExpenses = new List<object>();

                // 添加現金支出
                foreach (var e in cashExpenses)
                {
                    allExpenses.Add(new
                    {
                        Id = e.Id,
                        Amount = e.Amount,
                        Description = e.Description,
                        Category = e.Category ?? "其他",
                        Date = e.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                        ExpenseType = "Cash",
                        CanEdit = true,
                        CanDelete = true
                    });
                }

                // 取得信用卡支出
                var creditCardExpenses = await _creditCardRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);

                // 添加信用卡支出
                foreach (var e in creditCardExpenses)
                {
                    allExpenses.Add(new
                    {
                        Id = e.Id,
                        Amount = e.Amount,
                        Description = e.Description,
                        Category = e.Category ?? "其他",
                        Date = e.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                        ExpenseType = "CreditCard",
                        CanEdit = true,
                        CanDelete = true
                    });
                }

                // 按日期排序
                return allExpenses.OrderByDescending(e => DateTime.Parse(((dynamic)e).Date));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        // 新增缺少的方法實現

        public async Task<ExpenseResponse> AddCreditCardExpenseAsync(string userId, AddCreditCardExpenseRequest request)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var year = currentDate.Year;
                var month = currentDate.Month;

                // 步驟1：新增信用卡支出記錄
                var expense = new CreditCardExpense
                {
                    UserId = userId,
                    Year = year,
                    Month = month,
                    Amount = request.Amount,
                    Description = request.Description,
                    Category = request.Category ?? "未分類",
                    CardName = request.CardName ?? "預設信用卡",
                    Installments = request.Installments,
                    CreatedDate = currentDate,
                    UpdatedDate = null
                };

                await _creditCardRepository.AddAsync(expense);

                _logger.LogInformation(
                    "使用者 {UserId} 新增信用卡支出 {Amount}，卡片：{CardName}",
                    userId, request.Amount, expense.CardName);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "信用卡支出記錄新增成功",
                    ExpenseId = expense.Id,
                    RemainingBudget = 0 // 信用卡支出不影響現金預算
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增信用卡支出時發生錯誤，使用者：{UserId}", userId);
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "新增信用卡支出失敗，請稍後再試"
                };
            }
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(string userId, int expenseId, UpdateExpenseRequest request)
        {
            try
            {
                // 步驟1：查找支出記錄（先找現金支出，再找信用卡支出）
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var cashExpense = cashExpenses.FirstOrDefault();

                var creditCardExpenses = await _creditCardRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var creditCardExpense = creditCardExpenses.FirstOrDefault();

                if (cashExpense == null && creditCardExpense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄"
                    };
                }

                // 確定原始支出類型
                var originalExpenseType = cashExpense != null ? "Cash" : "CreditCard";
                var originalAmount = cashExpense?.Amount ?? creditCardExpense?.Amount ?? 0;
                var originalYear = cashExpense?.Year ?? creditCardExpense?.Year ?? 0;
                var originalMonth = cashExpense?.Month ?? creditCardExpense?.Month ?? 0;

                // 步驟2：處理類型轉換
                var requestExpenseTypeString = request.ExpenseType.ToString();
                if (originalExpenseType != requestExpenseTypeString)
                {
                    // 需要轉換類型：刪除原記錄，創建新記錄
                    return await HandleExpenseTypeConversion(userId, expenseId, request, 
                        originalExpenseType, originalAmount, originalYear, originalMonth);
                }

                // 步驟3：同類型更新
                if (requestExpenseTypeString == "Cash")
                {
                    return await UpdateCashExpense(userId, cashExpense!, request);
                }
                else
                {
                    return await UpdateCreditCardExpense(userId, creditCardExpense!, request);
                }
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

        private async Task<ExpenseResponse> HandleExpenseTypeConversion(string userId, int expenseId, 
            UpdateExpenseRequest request, string originalType, decimal originalAmount, int year, int month)
        {
            // 步驟1：刪除原記錄
            if (originalType == "Cash")
            {
                await _expenseRepository.DeleteAsync(expenseId);
                // 恢復現金預算
                await UpdateBudgetAmount(userId, year, month, originalAmount);
            }
            else
            {
                await _creditCardRepository.DeleteAsync(expenseId);
            }

            // 步驟2：創建新記錄
            if (request.ExpenseType == ExpenseType.Cash)
            {
                var newCashExpense = new CashExpense
                {
                    UserId = userId,
                    Year = year,
                    Month = month,
                    Amount = request.Amount,
                    Description = request.Description,
                    Category = request.Category ?? "其他",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _expenseRepository.AddAsync(newCashExpense);
                // 扣除現金預算
                await UpdateBudgetAmount(userId, year, month, -request.Amount);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄已轉換為現金支出",
                    ExpenseId = newCashExpense.Id,
                    RemainingBudget = await GetRemainingBudget(userId, year, month)
                };
            }
            else
            {
                var newCreditCardExpense = new CreditCardExpense
                {
                    UserId = userId,
                    Year = year,
                    Month = month,
                    Amount = request.Amount,
                    Description = request.Description,
                    Category = request.Category ?? "其他",
                    CardName = request.CardName ?? "預設信用卡",
                    Installments = request.Installments ?? 1,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _creditCardRepository.AddAsync(newCreditCardExpense);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "支出記錄已轉換為信用卡支出",
                    ExpenseId = newCreditCardExpense.Id,
                    RemainingBudget = await GetRemainingBudget(userId, year, month)
                };
            }
        }

        private async Task<ExpenseResponse> UpdateCashExpense(string userId, CashExpense expense, UpdateExpenseRequest request)
        {
            var amountDifference = request.Amount - expense.Amount;

            // 更新預算
            await UpdateBudgetAmount(userId, expense.Year, expense.Month, -amountDifference);

            // 更新記錄
            expense.Amount = request.Amount;
            expense.Description = request.Description;
            expense.Category = request.Category ?? expense.Category;
            expense.UpdatedDate = DateTime.UtcNow;

            await _expenseRepository.UpdateAsync(expense);

            return new ExpenseResponse
            {
                Success = true,
                Message = "現金支出記錄更新成功",
                ExpenseId = expense.Id,
                RemainingBudget = await GetRemainingBudget(userId, expense.Year, expense.Month)
            };
        }

        private async Task<ExpenseResponse> UpdateCreditCardExpense(string userId, CreditCardExpense expense, UpdateExpenseRequest request)
        {
            // 更新記錄
            expense.Amount = request.Amount;
            expense.Description = request.Description;
            expense.Category = request.Category ?? expense.Category;
            expense.CardName = request.CardName ?? expense.CardName;
            expense.Installments = request.Installments ?? expense.Installments;
            expense.UpdatedDate = DateTime.UtcNow;

            await _creditCardRepository.UpdateAsync(expense);

            return new ExpenseResponse
            {
                Success = true,
                Message = "信用卡支出記錄更新成功",
                ExpenseId = expense.Id,
                RemainingBudget = await GetRemainingBudget(userId, expense.Year, expense.Month)
            };
        }

        private async Task UpdateBudgetAmount(string userId, int year, int month, decimal amount)
        {
            var budgets = await _budgetRepository.FindAsync(
                b => b.UserId == userId && b.Year == year && b.Month == month);
            var budget = budgets.FirstOrDefault();

            if (budget != null)
            {
                budget.RemainingAmount += amount;
                budget.UpdatedDate = DateTime.UtcNow;
                await _budgetRepository.UpdateAsync(budget);
            }
        }

        private async Task<decimal> GetRemainingBudget(string userId, int year, int month)
        {
            var budgets = await _budgetRepository.FindAsync(
                b => b.UserId == userId && b.Year == year && b.Month == month);
            var budget = budgets.FirstOrDefault();
            return budget?.RemainingAmount ?? 0;
        }

        public async Task<ExpenseResponse> DeleteExpenseAsync(string userId, int expenseId)
        {
            try
            {
                // 步驟1：查找支出記錄（先找現金支出，再找信用卡支出）
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var cashExpense = cashExpenses.FirstOrDefault();

                var creditCardExpenses = await _creditCardRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var creditCardExpense = creditCardExpenses.FirstOrDefault();

                if (cashExpense == null && creditCardExpense == null)
                {
                    return new ExpenseResponse
                    {
                        Success = false,
                        Message = "找不到指定的支出記錄"
                    };
                }

                // 步驟2：刪除記錄並處理預算
                if (cashExpense != null)
                {
                    // 刪除現金支出：需要恢復預算
                    var budgets = await _budgetRepository.FindAsync(
                        b => b.UserId == userId && b.Year == cashExpense.Year && b.Month == cashExpense.Month);
                    var budget = budgets.FirstOrDefault();

                    if (budget != null)
                    {
                        budget.RemainingAmount += cashExpense.Amount;
                        budget.UpdatedDate = DateTime.UtcNow;
                        await _budgetRepository.UpdateAsync(budget);
                    }

                    await _expenseRepository.DeleteAsync(expenseId);

                    _logger.LogInformation(
                        "使用者 {UserId} 刪除現金支出記錄 {ExpenseId}，恢復預算 {Amount}",
                        userId, expenseId, cashExpense.Amount);

                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "現金支出記錄刪除成功",
                        RemainingBudget = budget?.RemainingAmount ?? 0
                    };
                }
                else
                {
                    // 刪除信用卡支出：不影響現金預算
                    await _creditCardRepository.DeleteAsync(expenseId);

                    _logger.LogInformation(
                        "使用者 {UserId} 刪除信用卡支出記錄 {ExpenseId}，金額 {Amount}",
                        userId, expenseId, creditCardExpense!.Amount);

                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "信用卡支出記錄刪除成功",
                        RemainingBudget = await GetRemainingBudget(userId, creditCardExpense.Year, creditCardExpense.Month)
                    };
                }
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
                        Category = expense.Category ?? "其他",
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