using Microsoft.EntityFrameworkCore;
using BA.Server.Data.Contexts;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// 資料庫相關的服務配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. 將資料庫配置邏輯集中管理
    /// 2. 方便未來新增或修改資料庫連接
    /// 3. 提高程式碼的可讀性和維護性
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// 註冊所有資料庫 DbContext
        /// 設計思維：採用微服務架構思維，每個功能模組有獨立的資料庫
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置物件</param>
        /// <param name="environment">環境資訊</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddDatabaseServices(
            this IServiceCollection services, 
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // 為什麼要傳入 environment？
            // 因為在開發環境需要啟用敏感資料記錄和詳細錯誤資訊
            // 這些功能在生產環境中會影響效能和安全性
            
            // 使用者資料庫配置
            services.AddDbContext<UserDbContext>(options =>
            {
                ConfigureSqliteContext(options, configuration, "DefaultConnection", environment);
            });

            // 支出管理資料庫配置
            services.AddDbContext<ExpenseDbContext>(options =>
            {
                ConfigureSqliteContext(options, configuration, "ExpenseDbConnection", environment);
            });

            // 維修記錄資料庫配置（目前註解，未來可啟用）
            // services.AddDbContext<MaintenanceDbContext>(options =>
            // {
            //     ConfigureSqliteContext(options, configuration, "MaintenanceConnection", environment);
            // });

            return services;
        }

        /// <summary>
        /// 配置 SQLite 資料庫選項的通用方法
        /// 為什麼要抽取成獨立方法？
        /// 1. 避免重複程式碼
        /// 2. 統一配置邏輯
        /// 3. 方便未來修改資料庫配置
        /// </summary>
        /// <param name="options">DbContext 選項建構器</param>
        /// <param name="configuration">配置物件</param>
        /// <param name="connectionStringName">連接字串名稱</param>
        /// <param name="environment">環境資訊</param>
        private static void ConfigureSqliteContext(
            DbContextOptionsBuilder options,
            IConfiguration configuration,
            string connectionStringName,
            IWebHostEnvironment environment)
        {
            // 設定 SQLite 連接字串和 Migration 組件
            // MigrationsAssembly 指定 Migration 檔案的位置
            options.UseSqlite(
                configuration.GetConnectionString(connectionStringName),
                sqliteOptions => sqliteOptions.MigrationsAssembly("BA.Server.Data"));

            // 開發環境專用設定
            if (environment.IsDevelopment())
            {
                // EnableSensitiveDataLogging：記錄敏感資料（如參數值）
                // 注意：生產環境不應啟用，因為會記錄敏感資訊
                options.EnableSensitiveDataLogging();
                
                // EnableDetailedErrors：提供詳細的錯誤資訊
                // 幫助開發時除錯，但在生產環境可能洩露系統資訊
                options.EnableDetailedErrors();
            }
        }

        /// <summary>
        /// 初始化所有資料庫
        /// 執行時機：應用程式啟動時
        /// 作用：確保資料庫存在且結構正確
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication InitializeDatabases(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // 1. 確保資料庫資料夾存在
                // 為什麼需要這步驟？SQLite 需要資料夾存在才能建立檔案
                EnsureDataDirectoryExists(app.Environment.ContentRootPath, logger);

                // 2. 初始化各個資料庫
                InitializeUserDatabase(scope.ServiceProvider, logger);
                InitializeExpenseDatabase(scope.ServiceProvider, logger);
                // InitializeMaintenanceDatabase(scope.ServiceProvider, logger); // 未來可啟用

                logger.LogInformation("所有資料庫初始化完成");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "資料庫初始化失敗");
                throw; // 重新拋出例外，讓應用程式無法啟動
            }

            return app;
        }

        /// <summary>
        /// 確保資料庫資料夾存在
        /// </summary>
        private static void EnsureDataDirectoryExists(string contentRootPath, ILogger logger)
        {
            var dataDirectory = Path.Combine(contentRootPath, "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
                logger.LogInformation("已建立資料庫資料夾：{DataDirectory}", dataDirectory);
            }
        }

        /// <summary>
        /// 初始化使用者資料庫
        /// </summary>
        private static void InitializeUserDatabase(IServiceProvider serviceProvider, ILogger logger)
        {
            var userContext = serviceProvider.GetRequiredService<UserDbContext>();
            
            // EnsureCreated vs Migrate 的差異：
            // EnsureCreated：如果資料庫不存在就建立，但不會套用 Migration
            // Migrate：套用所有待處理的 Migration，推薦用於生產環境
            userContext.Database.EnsureCreated();
            
            logger.LogInformation("使用者資料庫初始化完成");
        }

        /// <summary>
        /// 初始化支出管理資料庫
        /// </summary>
        private static void InitializeExpenseDatabase(IServiceProvider serviceProvider, ILogger logger)
        {
            var expenseContext = serviceProvider.GetRequiredService<ExpenseDbContext>();
            expenseContext.Database.EnsureCreated();
            
            logger.LogInformation("支出管理資料庫初始化完成");
        }

        // 未來可新增其他資料庫的初始化方法
        // private static void InitializeMaintenanceDatabase(IServiceProvider serviceProvider, ILogger logger)
        // {
        //     var maintenanceContext = serviceProvider.GetRequiredService<MaintenanceDbContext>();
        //     maintenanceContext.Database.EnsureCreated();
        //     
        //     logger.LogInformation("維修記錄資料庫初始化完成");
        // }
    }
}