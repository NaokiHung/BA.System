using Microsoft.Extensions.Logging;
using BA.Server.Core.DTOs.Expense;
using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Core.Enums;

namespace BA.Server.Business.Services
{
    /// <summary>
    /// 支出服務實作 - 增強版本
    /// 檔案路徑：BA.Server/BA.Server.Business/Services/ExpenseService.cs
    /// 
    /// 新增信用卡支出、編輯、刪除等功能的實作
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IBaseRepository<CashExpense> _expenseRepository;
        private readonly IBaseRepository<CreditCardExpense> _creditCardExpenseRepository;
        private readonly IBaseRepository<MonthlyBudget> _budgetRepository;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(
            IBaseRepository<CashExpense> expenseRepository,
            IBaseRepository<CreditCardExpense> creditCardExpenseRepository,
            IBaseRepository<MonthlyBudget> budgetRepository,
            ILogger<ExpenseService> logger)
        {
            _expenseRepository = expenseRepository;
            _creditCardExpenseRepository = creditCardExpenseRepository;
            _budgetRepository = budgetRepository;
            _logger = logger;
        }

        // === 現有方法（需要更新以包含 BudgetStatus） ===

        public async Task<MonthlyBudgetResponse> GetCurrentMonthBudgetAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.Now;
                var year = currentDate.Year;
                var month = currentDate.Month;

                // 取得當月預算
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var budget = budgets.FirstOrDefault();

                // 取得當月現金支出總額
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);
                var totalCashExpenses = cashExpenses.Sum(e => e.Amount);

                // 取得當月信用卡支出總額
                var creditCardExpenses = await _creditCardExpenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);
                var totalCreditCardExpenses = creditCardExpenses.Sum(e => e.Amount);

                // 計算預算狀態
                var budgetStatus = CalculateBudgetStatus(budget?.TotalAmount ?? 0, budget?.RemainingAmount ?? 0);

                // 計算月份名稱
                var monthName = GetMonthName(month);

                return new MonthlyBudgetResponse
                {
                    TotalBudget = budget?.TotalAmount ?? 0,
                    RemainingCash = budget?.RemainingAmount ?? 0,
                    TotalCashExpenses = totalCashExpenses,
                    TotalSubscriptions = 0, // 暫時設為0，後續實作訂閱功能時再修改
                    TotalCreditCard = totalCreditCardExpenses,
                    CombinedCreditTotal = totalCreditCardExpenses,
                    Year = year,
                    Month = month,
                    MonthName = monthName,
                    BudgetStatus = budgetStatus,
                    BudgetUtilizationPercentage = CalculateBudgetUtilization(budget?.TotalAmount ?? 0, budget?.RemainingAmount ?? 0),
                    RemainingDaysInMonth = CalculateRemainingDays(currentDate),
                    AverageDailyBudget = CalculateAverageDailyBudget(budget?.RemainingAmount ?? 0, currentDate)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得當月預算資訊時發生錯誤，使用者：{UserId}", userId);
                throw;
            }
        }

        // === 現有的其他方法保持不變 ===

        public async Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request)
        {
            // 現有實作保持不變
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
                    Category = request.Category ?? "其他",
                    CreatedDate = currentDate
                };

                await _expenseRepository.AddAsync(expense);

                // 步驟4：更新預算餘額
                budget.RemainingAmount -= request.Amount;
                budget.UpdatedDate = currentDate;
                await _budgetRepository.UpdateAsync(budget);

                _logger.LogInformation(
                    "使用者 {UserId} 新增現金支出 {Amount}，剩餘預算 {Remaining}",
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

        // === 新增的方法實作 ===

        public async Task<ExpenseResponse> AddCreditCardExpenseAsync(string userId, AddCreditCardExpenseRequest request)
        {
            try
            {
                var currentDate = DateTime.Now;
                var year = currentDate.Year;
                var month = currentDate.Month;

                // 新增信用卡支出記錄
                var expense = new CreditCardExpense
                {
                    UserId = userId,
                    Year = year,
                    Month = month,
                    Amount = request.Amount,
                    Description = request.Description,
                    Category = request.Category ?? "其他",
                    CardName = request.CardName,
                    Installments = request.Installments,
                    IsOnlineTransaction = request.IsOnlineTransaction,
                    MerchantName = request.MerchantName,
                    CreatedDate = currentDate
                };

                await _creditCardExpenseRepository.AddAsync(expense);

                _logger.LogInformation(
                    "使用者 {UserId} 新增信用卡支出 {Amount}，信用卡：{CardName}",
                    userId, request.Amount, request.CardName);

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
                if (request.ExpenseType == "Cash")
                {
                    return await UpdateCashExpenseAsync(userId, expenseId, request);
                }
                else if (request.ExpenseType == "CreditCard")
                {
                    return await UpdateCreditCardExpenseAsync(userId, expenseId, request);
                }

                return new ExpenseResponse
                {
                    Success = false,
                    Message = "不支援的支出類型"
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
                // 先嘗試從現金支出中找
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var cashExpense = cashExpenses.FirstOrDefault();

                if (cashExpense != null)
                {
                    return await DeleteCashExpenseAsync(userId, cashExpense);
                }

                // 再嘗試從信用卡支出中找
                var creditCardExpenses = await _creditCardExpenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var creditCardExpense = creditCardExpenses.FirstOrDefault();

                if (creditCardExpense != null)
                {
                    return await DeleteCreditCardExpenseAsync(userId, creditCardExpense);
                }

                return new ExpenseResponse
                {
                    Success = false,
                    Message = "找不到指定的支出記錄"
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

        public async Task<ExpenseDetailResponse?> GetExpenseDetailAsync(string userId, int expenseId)
        {
            try
            {
                // 先嘗試從現金支出中找
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var cashExpense = cashExpenses.FirstOrDefault();

                if (cashExpense != null)
                {
                    return new ExpenseDetailResponse
                    {
                        Id = cashExpense.Id,
                        Amount = cashExpense.Amount,
                        Description = cashExpense.Description,
                        Category = cashExpense.Category ?? "其他",
                        ExpenseType = "Cash",
                        CreatedDate = cashExpense.CreatedDate,
                        Year = cashExpense.Year,
                        Month = cashExpense.Month,
                        UserId = cashExpense.UserId,
                        CanEdit = CanEditExpense(cashExpense.CreatedDate),
                        CanDelete = CanDeleteExpense(cashExpense.CreatedDate)
                    };
                }

                // 再嘗試從信用卡支出中找
                var creditCardExpenses = await _creditCardExpenseRepository.FindAsync(
                    e => e.Id == expenseId && e.UserId == userId);
                var creditCardExpense = creditCardExpenses.FirstOrDefault();

                if (creditCardExpense != null)
                {
                    return new ExpenseDetailResponse
                    {
                        Id = creditCardExpense.Id,
                        Amount = creditCardExpense.Amount,
                        Description = creditCardExpense.Description,
                        Category = creditCardExpense.Category ?? "其他",
                        ExpenseType = "CreditCard",
                        CardName = creditCardExpense.CardName,
                        Installments = creditCardExpense.Installments,
                        MerchantName = creditCardExpense.MerchantName,
                        IsOnlineTransaction = creditCardExpense.IsOnlineTransaction,
                        CreatedDate = creditCardExpense.CreatedDate,
                        Year = creditCardExpense.Year,
                        Month = creditCardExpense.Month,
                        UserId = creditCardExpense.UserId,
                        CanEdit = CanEditExpense(creditCardExpense.CreatedDate),
                        CanDelete = CanDeleteExpense(creditCardExpense.CreatedDate)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出記錄詳情時發生錯誤，使用者：{UserId}，支出ID：{ExpenseId}", userId, expenseId);
                return null;
            }
        }

        public async Task<IEnumerable<ExpenseHistory>> GetExpenseHistoryAsync(string userId, int year, int month)
        {
            try
            {
                var result = new List<ExpenseHistory>();

                // 取得現金支出記錄
                var cashExpenses = await _expenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);

                foreach (var expense in cashExpenses)
                {
                    result.Add(new ExpenseHistory
                    {
                        Id = expense.Id,
                        Amount = expense.Amount,
                        Description = expense.Description,
                        Category = expense.Category ?? "其他",
                        Date = expense.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ExpenseType = "Cash",
                        Year = expense.Year,
                        Month = expense.Month,
                        CanEdit = CanEditExpense(expense.CreatedDate),
                        CanDelete = CanDeleteExpense(expense.CreatedDate)
                    });
                }

                // 取得信用卡支出記錄
                var creditCardExpenses = await _creditCardExpenseRepository.FindAsync(
                    e => e.UserId == userId && e.Year == year && e.Month == month);

                foreach (var expense in creditCardExpenses)
                {
                    result.Add(new ExpenseHistory
                    {
                        Id = expense.Id,
                        Amount = expense.Amount,
                        Description = expense.Description,
                        Category = expense.Category ?? "其他",
                        Date = expense.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ExpenseType = "CreditCard",
                        CardName = expense.CardName,
                        Installments = expense.Installments ?? 1,
                        MerchantName = expense.MerchantName,
                        IsOnlineTransaction = expense.IsOnlineTransaction,
                        Year = expense.Year,
                        Month = expense.Month,
                        CanEdit = CanEditExpense(expense.CreatedDate),
                        CanDelete = CanDeleteExpense(expense.CreatedDate)
                    });
                }

                // 按日期排序（最新的在前）
                return result.OrderByDescending(e => e.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得支出歷史記錄時發生錯誤，使用者：{UserId}", userId);
                return Enumerable.Empty<ExpenseHistory>();
            }
        }

        // 其他現有方法保持不變...
        public async Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month)
        {
            // 現有實作保持不變
            try
            {
                var budgets = await _budgetRepository.FindAsync(
                    b => b.UserId == userId && b.Year == year && b.Month == month);
                var existingBudget = budgets.FirstOrDefault();

                if (existingBudget != null)
                {
                    // 更新現有預算
                    var difference = amount - existingBudget.TotalAmount;
                    existingBudget.TotalAmount = amount;
                    existingBudget.RemainingAmount += difference;
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
                    var newBudget = new MonthlyBudget
                    {
                        UserId = userId,
                        Year = year,
                        Month = month,
                        TotalAmount = amount,
                        RemainingAmount = amount,
                        CreatedDate = DateTime.Now
                    };
                    
                    await _budgetRepository.AddAsync(newBudget);
                    
                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "預算設定成功",
                        RemainingBudget = amount
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

        // === 私有輔助方法 ===

        private string CalculateBudgetStatus(decimal totalBudget, decimal remainingBudget)
        {
            if (totalBudget <= 0) return "Unknown";
            
            var utilizationPercentage = ((totalBudget - remainingBudget) / totalBudget) * 100;
            
            return utilizationPercentage switch
            {
                < 70 => "Healthy",
                < 90 => "Warning",
                _ => "Exceeded"
            };
        }

        private decimal CalculateBudgetUtilization(decimal totalBudget, decimal remainingBudget)
        {
            if (totalBudget <= 0) return 0;
            return Math.Round(((totalBudget - remainingBudget) / totalBudget) * 100, 2);
        }

        private int CalculateRemainingDays(DateTime currentDate)
        {
            var lastDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 
                DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            return (lastDayOfMonth - currentDate).Days + 1;
        }

        private decimal CalculateAverageDailyBudget(decimal remainingBudget, DateTime currentDate)
        {
            var remainingDays = CalculateRemainingDays(currentDate);
            return remainingDays > 0 ? Math.Round(remainingBudget / remainingDays, 2) : 0;
        }

        private string GetMonthName(int month)
        {
            var monthNames = new[] 
            {
                "", "一月", "二月", "三月", "四月", "五月", "六月",
                "七月", "八月", "九月", "十月", "十一月", "十二月"
            };
            return month >= 1 && month <= 12 ? monthNames[month] : "未知";
        }

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

        private async Task<ExpenseResponse> UpdateCashExpenseAsync(string userId, int expenseId, UpdateExpenseRequest request)
        {
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
            expense.UpdatedDate = DateTime.Now;

            await _expenseRepository.UpdateAsync(expense);

            _logger.LogInformation(
                "使用者 {UserId} 更新現金支出 {ExpenseId}，金額變化：{AmountDifference}",
                userId, expenseId, amountDifference);

            return new ExpenseResponse
            {
                Success = true,
                Message = "支出記錄更新成功",
                ExpenseId = expense.Id,
                RemainingBudget = 0 // 前端會重新載入預算資訊
            };
        }

        private async Task<ExpenseResponse> UpdateCreditCardExpenseAsync(string userId, int expenseId, UpdateExpenseRequest request)
        {
            var expenses = await _creditCardExpenseRepository.FindAsync(
                e => e.Id == expenseId && e.UserId == userId);
            var expense = expenses.FirstOrDefault();

            if (expense == null)
            {
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "找不到指定的信用卡支出記錄"
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

            // 更新信用卡支出記錄
            expense.Amount = request.Amount;
            expense.Description = request.Description;
            expense.Category = request.Category ?? "其他";
            expense.CardName = request.CardName;
            expense.Installments = request.Installments;
            expense.MerchantName = request.MerchantName;
            expense.IsOnlineTransaction = request.IsOnlineTransaction ?? false;
            expense.UpdatedDate = DateTime.Now;

            await _creditCardExpenseRepository.UpdateAsync(expense);

            _logger.LogInformation(
                "使用者 {UserId} 更新信用卡支出 {ExpenseId}",
                userId, expenseId);

            return new ExpenseResponse
            {
                Success = true,
                Message = "信用卡支出記錄更新成功",
                ExpenseId = expense.Id,
                RemainingBudget = 0 // 信用卡支出不影響現金預算
            };
        }

        private async Task<ExpenseResponse> DeleteCashExpenseAsync(string userId, CashExpense expense)
        {
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

            // 刪除支出記錄
            await _expenseRepository.DeleteAsync(expense);

            _logger.LogInformation(
                "使用者 {UserId} 刪除現金支出 {ExpenseId}，退還金額：{Amount}",
                userId, expense.Id, expense.Amount);

            return new ExpenseResponse
            {
                Success = true,
                Message = "支出記錄已刪除，金額已退還至預算",
                RemainingBudget = budget?.RemainingAmount ?? 0
            };
        }

        private async Task<ExpenseResponse> DeleteCreditCardExpenseAsync(string userId, CreditCardExpense expense)
        {
            // 檢查是否可刪除
            if (!CanDeleteExpense(expense.CreatedDate))
            {
                return new ExpenseResponse
                {
                    Success = false,
                    Message = "該支出記錄已超過可刪除期限"
                };
            }

            // 刪除信用卡支出記錄
            await _creditCardExpenseRepository.DeleteAsync(expense);

            _logger.LogInformation(
                "使用者 {UserId} 刪除信用卡支出 {ExpenseId}",
                userId, expense.Id);

            return new ExpenseResponse
            {
                Success = true,
                Message = "信用卡支出記錄已刪除",
                RemainingBudget = 0 // 信用卡支出不影響現金預算
            };
        }
    }
}