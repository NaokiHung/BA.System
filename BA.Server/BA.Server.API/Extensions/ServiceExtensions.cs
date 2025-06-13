using BA.Server.Business.Services;
using BA.Server.Core.Interfaces;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// 業務服務層依賴注入的擴充方法
    /// 為什麼要獨立管理 Service 註冊？
    /// 1. 業務邏輯層是核心功能，需要清楚管理
    /// 2. 方便新增新的業務服務
    /// 3. 統一管理服務的生命週期
    /// 4. 符合分層應用程式架構的設計原則
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// 註冊所有業務服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // 為什麼業務服務使用 Scoped 生命週期？
            // 1. 業務服務通常需要在單一請求中保持狀態一致性
            // 2. 業務服務依賴 Repository（Scoped），所以也應該是 Scoped
            // 3. 避免跨請求的業務邏輯污染

            // 認證和授權相關服務
            RegisterAuthenticationServices(services);

            // 支出管理相關服務
            RegisterExpenseServices(services);

            // 維修記錄相關服務（未來可啟用）
            // RegisterMaintenanceServices(services);

            // 通用服務
            RegisterCommonServices(services);

            return services;
        }

        /// <summary>
        /// 註冊認證和授權相關服務
        /// 為什麼要分開註冊？
        /// 1. 不同領域的服務分開管理
        /// 2. 方便維護和擴展
        /// 3. 符合領域驅動設計 (DDD) 的概念
        /// </summary>
        private static void RegisterAuthenticationServices(IServiceCollection services)
        {
            // 認證服務
            // 負責：使用者登入、註冊、Token 生成等
            services.AddScoped<IAuthService, AuthService>();

            // 未來可能新增的認證相關服務
            // services.AddScoped<IPasswordService, PasswordService>();           // 密碼加密、驗證
            // services.AddScoped<IEmailVerificationService, EmailVerificationService>(); // 郵件驗證
            // services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>(); // 兩階段驗證
        }

        /// <summary>
        /// 註冊支出管理相關服務
        /// </summary>
        private static void RegisterExpenseServices(IServiceCollection services)
        {
            // 支出管理服務
            // 負責：支出記錄、預算管理、統計分析等
            services.AddScoped<IExpenseService, ExpenseService>();

            // 未來可能新增的支出相關服務
            // services.AddScoped<IBudgetService, BudgetService>();             // 預算管理
            // services.AddScoped<ICategoryService, CategoryService>();         // 分類管理
            // services.AddScoped<IReportService, ReportService>();             // 報表生成
            // services.AddScoped<IExpenseAnalysisService, ExpenseAnalysisService>(); // 支出分析
        }

        /// <summary>
        /// 註冊維修記錄相關服務（未來可啟用）
        /// </summary>
        // private static void RegisterMaintenanceServices(IServiceCollection services)
        // {
        //     // 維修記錄服務
        //     services.AddScoped<IMaintenanceService, MaintenanceService>();
        //
        //     // 設備管理服務
        //     services.AddScoped<IEquipmentService, EquipmentService>();
        //
        //     // 維修排程服務
        //     services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
        // }

        /// <summary>
        /// 註冊通用服務
        /// 這些服務可能被多個業務領域使用
        /// </summary>
        private static void RegisterCommonServices(IServiceCollection services)
        {
            // HTTP 上下文存取器
            // 為什麼需要？在 Service 中可能需要取得 HTTP 請求資訊
            services.AddHttpContextAccessor();

            // 未來可能新增的通用服務
            // services.AddScoped<IEmailService, EmailService>();               // 郵件發送
            // services.AddScoped<IFileService, FileService>();                 // 檔案處理
            // services.AddScoped<ICacheService, CacheService>();               // 快取服務
            // services.AddScoped<INotificationService, NotificationService>(); // 通知服務
            // services.AddScoped<IAuditService, AuditService>();               // 稽核日誌
        }

        /// <summary>
        /// 註冊第三方整合服務
        /// 用於與外部系統整合的服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
        {
            // 範例：支付閘道整合
            // services.AddScoped<IPaymentService, StripePaymentService>();

            // 範例：雲端儲存服務
            // services.AddScoped<ICloudStorageService, AzureBlobStorageService>();

            // 範例：簡訊發送服務
            // services.AddScoped<ISmsService, TwilioSmsService>();

            // 範例：推播通知服務
            // services.AddScoped<IPushNotificationService, FirebasePushService>();

            return services;
        }

        /// <summary>
        /// 註冊背景服務和排程工作
        /// 用於處理非同步和排程任務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // 範例：資料備份排程服務
            // services.AddHostedService<DatabaseBackupService>();

            // 範例：郵件佇列處理服務
            // services.AddHostedService<EmailQueueProcessorService>();

            // 範例：過期資料清理服務
            // services.AddHostedService<DataCleanupService>();

            return services;
        }
    }
}