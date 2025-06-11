using BA.Server.Core.Entities;
using BA.Server.Core.Interfaces;
using BA.Server.Data.Contexts;
using BA.Server.Data.Repositories;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// Repository 依賴注入的擴充方法
    /// 提供兩種註冊方式：
    /// 1. 使用泛型 BaseRepository（簡單場景）
    /// 2. 使用具體 Repository 實作（複雜場景，推薦）
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// 註冊所有 Repository 服務
        /// 可以選擇使用具體實作或泛型實作
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="useSpecificImplementations">是否使用具體實作（推薦 true）</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddRepositoryServices(
            this IServiceCollection services, 
            bool useSpecificImplementations = true)
        {
            if (useSpecificImplementations)
            {
                // 推薦方式：使用具體的 Repository 實作
                RegisterSpecificRepositories(services);
            }
            else
            {
                // 簡單方式：使用泛型 BaseRepository
                RegisterGenericRepositories(services);
            }

            return services;
        }

        /// <summary>
        /// 註冊具體的 Repository 實作（推薦方式）
        /// 優點：
        /// 1. 型別安全
        /// 2. 可以新增專用方法
        /// 3. 更好的測試支援
        /// 4. 明確的依賴關係
        /// </summary>
        /// <param name="services">服務集合</param>
        private static void RegisterSpecificRepositories(IServiceCollection services)
        {
            // 使用者相關 Repository
            services.AddScoped<IUserRepository, UserRepository>();
            
            // 支出管理相關 Repository
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<ICreditCardExpenseRepository, CreditCardExpenseRepository>();
            services.AddScoped<IMonthlyBudgetRepository, MonthlyBudgetRepository>();

            // 同時保留泛型介面的註冊，以便既有程式碼不需要修改
            services.AddScoped<IBaseRepository<User>>(provider => 
                provider.GetRequiredService<IUserRepository>());
            services.AddScoped<IBaseRepository<CashExpense>>(provider => 
                provider.GetRequiredService<IExpenseRepository>());
            services.AddScoped<IBaseRepository<CreditCardExpense>>(provider => 
                provider.GetRequiredService<ICreditCardExpenseRepository>());
            services.AddScoped<IBaseRepository<MonthlyBudget>>(provider => 
                provider.GetRequiredService<IMonthlyBudgetRepository>());
        }

        /// <summary>
        /// 註冊泛型 Repository 實作（備用方式）
        /// 適用於簡單的 CRUD 操作，不需要特殊查詢方法
        /// </summary>
        /// <param name="services">服務集合</param>
        private static void RegisterGenericRepositories(IServiceCollection services)
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

            services.AddScoped<IBaseRepository<CreditCardExpense>>(provider =>
            {
                var context = provider.GetRequiredService<ExpenseDbContext>();
                var logger = provider.GetRequiredService<ILogger<BaseRepository<CreditCardExpense>>>();
                return new BaseRepository<CreditCardExpense>(context, logger);
            });
        }

        /// <summary>
        /// 驗證 Repository 配置的輔助方法
        /// 在開發環境中檢查所有 Repository 是否能正確建立
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>驗證結果和詳細資訊</returns>
        public static (bool IsValid, List<string> Messages) ValidateRepositoryConfiguration(IServiceProvider serviceProvider)
        {
            var messages = new List<string>();
            var isValid = true;

            try
            {
                using var scope = serviceProvider.CreateScope();
                var scopedProvider = scope.ServiceProvider;

                // 測試泛型 Repository
                var testResults = new Dictionary<string, bool>
                {
                    ["IBaseRepository<User>"] = TestRepository<User>(scopedProvider),
                    ["IBaseRepository<MonthlyBudget>"] = TestRepository<MonthlyBudget>(scopedProvider),
                    ["IBaseRepository<CashExpense>"] = TestRepository<CashExpense>(scopedProvider),
                    ["IBaseRepository<CreditCardExpense>"] = TestRepository<CreditCardExpense>(scopedProvider)
                };

                // 測試具體 Repository（如果已註冊）
                var specificTests = new Dictionary<string, Func<IServiceProvider, bool>>
                {
                    ["IUserRepository"] = provider => TestSpecificService<IUserRepository>(provider),
                    ["IExpenseRepository"] = provider => TestSpecificService<IExpenseRepository>(provider),
                    ["ICreditCardExpenseRepository"] = provider => TestSpecificService<ICreditCardExpenseRepository>(provider),
                    ["IMonthlyBudgetRepository"] = provider => TestSpecificService<IMonthlyBudgetRepository>(provider)
                };

                foreach (var test in specificTests)
                {
                    try
                    {
                        testResults[test.Key] = test.Value(scopedProvider);
                    }
                    catch
                    {
                        testResults[test.Key] = false;
                    }
                }

                // 產生驗證報告
                foreach (var result in testResults)
                {
                    if (result.Value)
                    {
                        messages.Add($"✅ {result.Key} 註冊成功");
                    }
                    else
                    {
                        messages.Add($"❌ {result.Key} 註冊失敗");
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    messages.Add("🎉 所有 Repository 配置驗證成功！");
                }
                else
                {
                    messages.Add("⚠️ 部分 Repository 配置有問題，請檢查 DI 設定");
                }
            }
            catch (Exception ex)
            {
                isValid = false;
                messages.Add($"💥 Repository 配置驗證時發生例外：{ex.Message}");
            }

            return (isValid, messages);
        }

        /// <summary>
        /// 測試泛型 Repository 是否能正確建立
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>是否成功</returns>
        private static bool TestRepository<T>(IServiceProvider serviceProvider) where T : class
        {
            try
            {
                var repository = serviceProvider.GetRequiredService<IBaseRepository<T>>();
                return repository != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 測試具體服務是否能正確建立
        /// </summary>
        /// <typeparam name="TService">服務類型</typeparam>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>是否成功</returns>
        private static bool TestSpecificService<TService>(IServiceProvider serviceProvider) where TService : class
        {
            try
            {
                var service = serviceProvider.GetRequiredService<TService>();
                return service != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得 Repository 配置摘要資訊
        /// 用於日誌記錄和監控
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>配置摘要</returns>
        public static string GetRepositoryConfigurationSummary(IServiceProvider serviceProvider)
        {
            var (isValid, messages) = ValidateRepositoryConfiguration(serviceProvider);
            return string.Join(Environment.NewLine, messages);
        }
    }
}