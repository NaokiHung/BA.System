using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Contexts;
using BA.Server.Data.Repositories;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// Repository 依賴注入的擴充方法
    /// 根據當前實體定義進行配置
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// 註冊所有 Repository 服務
        /// 只註冊目前存在的實體：User, MonthlyBudget, CashExpense
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // 使用者相關的 Repository（使用 UserDbContext）
            services.AddScoped<IBaseRepository<User>>(provider =>
            {
                var context = provider.GetRequiredService<UserDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<User>>>();
                return new BaseRepository<User>(context, logger);
            });

            // 支出管理相關的 Repository（使用 ExpenseDbContext）
            services.AddScoped<IBaseRepository<MonthlyBudget>>(provider =>
            {
                var context = provider.GetRequiredService<ExpenseDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<MonthlyBudget>>>();
                return new BaseRepository<MonthlyBudget>(context, logger);
            });

            services.AddScoped<IBaseRepository<CashExpense>>(provider =>
            {
                var context = provider.GetRequiredService<ExpenseDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<CashExpense>>>();
                return new BaseRepository<CashExpense>(context, logger);
            });

            // 未來如果新增 CreditCardExpense 實體，可以在此處新增：
            // services.AddScoped<IBaseRepository<CreditCardExpense>>(provider =>
            // {
            //     var context = provider.GetRequiredService<ExpenseDbContext>();
            //     var logger = provider.GetRequiredService<ILogger<BaseRepository<CreditCardExpense>>>();
            //     return new BaseRepository<CreditCardExpense>(context, logger);
            // });

            return services;
        }

        /// <summary>
        /// 驗證 Repository 配置
        /// 只驗證目前存在的實體
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>驗證結果</returns>
        public static bool ValidateRepositoryConfiguration(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                
                var userRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<User>>();
                var budgetRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<MonthlyBudget>>();
                var cashExpenseRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<CashExpense>>();

                return userRepo != null && budgetRepo != null && cashExpenseRepo != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}