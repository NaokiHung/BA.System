using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Contexts;
using BA.Server.Data.Repositories;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// Repository 依賴注入的擴充方法
    /// 為什麼要獨立管理 Repository 註冊？
    /// 1. 集中管理資料存取層的依賴注入
    /// 2. 方便新增新的 Repository
    /// 3. 統一管理生命週期配置
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// 註冊所有 Repository 服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // 為什麼所有 Repository 都使用 Scoped 生命週期？
            // 1. DbContext 預設是 Scoped 生命週期
            // 2. Repository 依賴 DbContext，所以也必須是 Scoped
            // 3. 確保在同一個 HTTP 請求中使用相同的 DbContext 實例
            // 4. 避免跨請求的資料污染問題

            // 使用者相關的 Repository（使用 UserDbContext）
            RegisterUserRepositories(services);

            // 支出管理相關的 Repository（使用 ExpenseDbContext）
            RegisterExpenseRepositories(services);

            // 維修記錄相關的 Repository（未來可啟用）
            // RegisterMaintenanceRepositories(services);

            return services;
        }

        /// <summary>
        /// 註冊使用者相關的 Repository
        /// 為什麼要分開註冊？
        /// 1. 不同的 Repository 使用不同的 DbContext
        /// 2. 方便管理和維護
        /// 3. 符合單一職責原則
        /// </summary>
        private static void RegisterUserRepositories(IServiceCollection services)
        {
            // 使用者 Repository
            // 注意：這裡使用工廠模式來注入正確的 DbContext
            services.AddScoped<IBaseRepository<User>>(provider =>
            {
                // 為什麼要手動從 DI 容器取得服務？
                // 因為 BaseRepository<User> 需要 UserDbContext，
                // 但 DI 容器無法自動推斷要注入哪一個 DbContext
                var context = provider.GetRequiredService<UserDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<User>>>();
                
                return new BaseRepository<User>(context, logger);
            });

            // 如果有其他使用 UserDbContext 的實體，可以在此處新增
            // 例如：UserProfile, UserSettings 等
        }

        /// <summary>
        /// 註冊支出管理相關的 Repository
        /// </summary>
        private static void RegisterExpenseRepositories(IServiceCollection services)
        {
            // 月度預算 Repository
            services.AddScoped<IBaseRepository<MonthlyBudget>>(provider =>
            {
                var context = provider.GetRequiredService<ExpenseDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<MonthlyBudget>>>();
                
                return new BaseRepository<MonthlyBudget>(context, logger);
            });

            // 現金支出 Repository
            services.AddScoped<IBaseRepository<CashExpense>>(provider =>
            {
                var context = provider.GetRequiredService<ExpenseDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<CashExpense>>>();
                
                return new BaseRepository<CashExpense>(context, logger);
            });

            // 未來可能新增的支出相關實體 Repository
            // 例如：CreditCardExpense, Category, Tag 等
        }

        /// <summary>
        /// 註冊維修記錄相關的 Repository（未來可啟用）
        /// </summary>
        // private static void RegisterMaintenanceRepositories(IServiceCollection services)
        // {
        //     // 維修記錄 Repository
        //     services.AddScoped<IBaseRepository<MaintenanceRecord>>(provider =>
        //     {
        //         var context = provider.GetRequiredService<MaintenanceDbContext>();
        //         var logger = provider.GetRequiredService<ILogger<BaseRepository<MaintenanceRecord>>>();
        //         
        //         return new BaseRepository<MaintenanceRecord>(context, logger);
        //     });
        //
        //     // 設備資料 Repository
        //     services.AddScoped<IBaseRepository<Equipment>>(provider =>
        //     {
        //         var context = provider.GetRequiredService<MaintenanceDbContext>();
        //         var logger = provider.GetRequiredService<ILogger<BaseRepository<Equipment>>>();
        //         
        //         return new BaseRepository<Equipment>(context, logger);
        //     });
        // }

        /// <summary>
        /// 進階用法：註冊特定的 Repository 介面實作
        /// 當您需要為特定實體建立專門的 Repository 時使用
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddSpecializedRepositories(this IServiceCollection services)
        {
            // 範例：如果您有專門的 UserRepository 介面和實作
            // services.AddScoped<IUserRepository, UserRepository>();
            
            // 範例：如果您有專門的 ExpenseRepository 介面和實作
            // services.AddScoped<IExpenseRepository, ExpenseRepository>();

            return services;
        }
    }
}