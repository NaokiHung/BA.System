using BA.Server.Core.DTOs.Expense;

namespace BA.Server.Core.Interfaces
{
    public interface IExpenseService
    {
        Task<MonthlyBudgetResponse> GetCurrentMonthBudgetAsync(string userId);
        Task<ExpenseResponse> AddCashExpenseAsync(string userId, AddCashExpenseRequest request);
        Task<ExpenseResponse> SetMonthlyBudgetAsync(string userId, decimal amount, int year, int month);
        Task<IEnumerable<object>> GetExpenseHistoryAsync(string userId, int year, int month);
    }
}