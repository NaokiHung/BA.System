using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Repositories;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// Repository 層依賴注入的擴充方法
    /// 檔案路徑：BA.Server/BA.Server.API/Extensions/RepositoryExtensions.cs
    /// 
    /// 新增信用卡支出 Repository 的註冊
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// 註冊所有 Repository
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // 為什麼 Repository 使用 Scoped 生命週期？
            // 1. Repository 通常與 DbContext 的生命週期相同
            // 2. 確保在同一個請求中使用相同的資料庫連線
            // 3. 支援工作單元模式 (Unit of Work)

            // 認證相關 Repository
            services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();

            // 支出管理相關 Repository
            services.AddScoped<IBaseRepository<CashExpense>, BaseRepository<CashExpense>>();
            services.AddScoped<IBaseRepository<CreditCardExpense>, BaseRepository<CreditCardExpense>>();
            services.AddScoped<IBaseRepository<MonthlyBudget>, BaseRepository<MonthlyBudget>>();

            // 未來可能新增的 Repository
            // services.AddScoped<IBaseRepository<Subscription>, BaseRepository<Subscription>>();
            // services.AddScoped<IBaseRepository<Category>, BaseRepository<Category>>();
            // services.AddScoped<IBaseRepository<PaymentMethod>, BaseRepository<PaymentMethod>>();
            // services.AddScoped<IBaseRepository<Receipt>, BaseRepository<Receipt>>();

            return services;
        }
    }

    /// <summary>
    /// 在 Program.cs 或 Startup.cs 中的使用方式：
    /// 
    /// builder.Services.AddRepositories();
    /// 
    /// 這樣就會自動註冊所有必要的 Repository
    /// </summary>
}