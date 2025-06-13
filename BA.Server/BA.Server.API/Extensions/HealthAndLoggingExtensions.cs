using BA.Server.Data.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// 健康檢查和日誌配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. 監控和日誌是應用程式的重要基礎設施
    /// 2. 方便根據環境調整監控策略
    /// 3. 統一管理所有健康檢查項目
    /// </summary>
    public static class HealthAndLoggingExtensions
    {
        /// <summary>
        /// 設定健康檢查服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            // 資料庫健康檢查
            ConfigureDatabaseHealthChecks(healthChecksBuilder);

            // 外部服務健康檢查
            ConfigureExternalServiceHealthChecks(healthChecksBuilder);

            // 系統資源健康檢查
            ConfigureSystemHealthChecks(healthChecksBuilder);

            return services;
        }

        /// <summary>
        /// 配置資料庫健康檢查
        /// 為什麼需要？
        /// 1. 確保應用程式能正常連接資料庫
        /// 2. 監控資料庫回應時間
        /// 3. 在負載平衡器中作為健康檢查依據
        /// </summary>
        private static void ConfigureDatabaseHealthChecks(IHealthChecksBuilder builder)
        {
            // 使用者資料庫健康檢查
            builder.AddDbContextCheck<UserDbContext>(
                name: "user-database",
                failureStatus: HealthStatus.Degraded, // 失敗時標記為降級而非不健康
                tags: new[] { "database", "user" });

            // 支出管理資料庫健康檢查
            builder.AddDbContextCheck<ExpenseDbContext>(
                name: "expense-database",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "database", "expense" });

            // 維修記錄資料庫健康檢查（未來可啟用）
            // builder.AddDbContextCheck<MaintenanceDbContext>(
            //     name: "maintenance-database",
            //     failureStatus: HealthStatus.Degraded,
            //     tags: new[] { "database", "maintenance" });
        }

        /// <summary>
        /// 配置外部服務健康檢查
        /// </summary>
        private static void ConfigureExternalServiceHealthChecks(IHealthChecksBuilder builder)
        {
            // 範例：HTTP 服務健康檢查
            // builder.AddUrlGroup(
            //     new Uri("https://api.external-service.com/health"),
            //     name: "external-api",
            //     failureStatus: HealthStatus.Degraded,
            //     tags: new[] { "external", "api" });

            // 範例：Redis 健康檢查
            // builder.AddRedis(
            //     connectionString: "localhost:6379",
            //     name: "redis-cache",
            //     failureStatus: HealthStatus.Degraded,
            //     tags: new[] { "cache", "redis" });

            // 範例：SMTP 服務健康檢查
            // builder.AddSmtpHealthCheck(
            //     options =>
            //     {
            //         options.Host = "smtp.gmail.com";
            //         options.Port = 587;
            //     },
            //     name: "smtp-service",
            //     tags: new[] { "email", "smtp" });
        }

        /// <summary>
        /// 配置系統資源健康檢查
        /// </summary>
        private static void ConfigureSystemHealthChecks(IHealthChecksBuilder builder)
        {
            // 範例：記憶體使用量檢查
            // builder.AddProcessAllocatedMemoryHealthCheck(
            //     maximumMegabytesAllocated: 1024, // 1GB
            //     name: "memory-usage",
            //     tags: new[] { "system", "memory" });

            // 範例：磁碟空間檢查
            // builder.AddDiskStorageHealthCheck(
            //     options =>
            //     {
            //         options.AddDrive("C:\\", minimumFreeMegabytes: 1024); // 1GB
            //     },
            //     name: "disk-storage",
            //     tags: new[] { "system", "disk" });
        }

        /// <summary>
        /// 設定日誌服務
        /// </summary>
        /// <param name="builder">Web 應用程式建構器</param>
        /// <returns>Web 應用程式建構器（支援鏈式呼叫）</returns>
        public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
        {
            // 清除預設的日誌提供者
            builder.Logging.ClearProviders();

            // 根據環境配置不同的日誌提供者
            ConfigureLoggingProviders(builder.Logging, builder.Environment);

            // 設定日誌層級
            ConfigureLogLevels(builder.Logging, builder.Environment);

            // 配置結構化日誌（如果需要）
            ConfigureStructuredLogging(builder.Services, builder.Configuration);

            return builder;
        }

        /// <summary>
        /// 配置日誌提供者
        /// 為什麼要根據環境調整？
        /// 1. 開發環境需要詳細的除錯資訊
        /// 2. 生產環境需要效能優化的日誌
        /// 3. 不同環境可能使用不同的日誌收集系統
        /// </summary>
        private static void ConfigureLoggingProviders(ILoggingBuilder logging, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                // 開發環境：控制台和除錯輸出
                logging.AddConsole();
                logging.AddDebug();
            }
            else if (environment.IsProduction())
            {
                // 生產環境：結構化日誌和遠端收集
                logging.AddConsole();
                
                // 範例：新增 Application Insights
                // logging.AddApplicationInsights();
                
                // 範例：新增 Serilog
                // logging.AddSerilog();
            }
            else
            {
                // 測試或其他環境
                logging.AddConsole();
            }
        }

        /// <summary>
        /// 配置日誌層級
        /// </summary>
        private static void ConfigureLogLevels(ILoggingBuilder logging, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                // 開發環境：顯示詳細的資料庫查詢資訊
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
                
                // 顯示 HTTP 請求詳細資訊
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Debug);
                
                // 應用程式日誌
                logging.AddFilter("BA.Server", LogLevel.Debug);
            }
            else
            {
                // 生產環境：降低日誌層級以提升效能
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                
                // 只記錄應用程式的重要訊息
                logging.AddFilter("BA.Server", LogLevel.Information);
                
                // 系統層級日誌
                logging.AddFilter("System", LogLevel.Warning);
                logging.AddFilter("Microsoft", LogLevel.Warning);
            }
        }

        /// <summary>
        /// 配置結構化日誌
        /// 為什麼需要結構化日誌？
        /// 1. 方便日誌分析和查詢
        /// 2. 支援日誌聚合和監控
        /// 3. 提供更好的可觀測性
        /// </summary>
        private static void ConfigureStructuredLogging(IServiceCollection services, IConfiguration configuration)
        {
            // 範例：配置 Serilog
            // services.AddSerilog((serviceProvider, loggerConfiguration) =>
            // {
            //     loggerConfiguration
            //         .ReadFrom.Configuration(configuration)
            //         .Enrich.FromLogContext()
            //         .Enrich.WithMachineName()
            //         .Enrich.WithEnvironmentUserName()
            //         .WriteTo.Console(outputTemplate: 
            //             "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            //         .WriteTo.File("logs/application-.log", 
            //             rollingInterval: RollingInterval.Day,
            //             retainedFileCountLimit: 30);
            // });
        }

        /// <summary>
        /// 配置健康檢查中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseHealthCheckEndpoints(this WebApplication app)
        {
            // 基本健康檢查端點
            app.MapHealthChecks("/health");

            // 詳細健康檢查端點（包含檢查結果詳情）
            app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = WriteDetailedHealthCheckResponse,
                AllowCachingResponses = false
            });

            // 按標籤分組的健康檢查
            app.MapHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("database"),
                ResponseWriter = WriteDetailedHealthCheckResponse
            });

            app.MapHealthChecks("/health/external", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("external"),
                ResponseWriter = WriteDetailedHealthCheckResponse
            });

            return app;
        }

        /// <summary>
        /// 撰寫詳細的健康檢查回應
        /// 為什麼要自訂回應格式？
        /// 1. 提供結構化的健康檢查資訊
        /// 2. 方便監控系統解析
        /// 3. 包含更多有用的診斷資訊
        /// </summary>
        private static async Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Timestamp = DateTime.UtcNow,
                Results = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Description = entry.Value.Description,
                    Data = entry.Value.Data,
                    Tags = entry.Value.Tags,
                    Exception = entry.Value.Exception?.Message
                })
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// 新增效能監控
        /// 使用時機：當需要監控應用程式效能時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置物件</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddPerformanceMonitoring(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // 範例：Application Insights
            // services.AddApplicationInsightsTelemetry(configuration);

            // 範例：自訂效能計數器
            // services.AddSingleton<IPerformanceCounterService, PerformanceCounterService>();

            return services;
        }

        /// <summary>
        /// 新增應用程式啟動資訊記錄
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication LogStartupInformation(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var environment = app.Environment.EnvironmentName;
            var urls = app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "http://localhost:5000";

            // 記錄啟動資訊
            logger.LogInformation("=".PadRight(60, '='));
            logger.LogInformation("個人管理系統 API 啟動完成");
            logger.LogInformation("環境：{Environment}", environment);
            logger.LogInformation("API 端點：{Urls}", urls);
            logger.LogInformation("程序識別碼：{ProcessId}", Environment.ProcessId);
            logger.LogInformation("機器名稱：{MachineName}", Environment.MachineName);
            logger.LogInformation("作業系統：{OSVersion}", Environment.OSVersion);
            logger.LogInformation("CLR 版本：{CLRVersion}", Environment.Version);

            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("Swagger UI：{SwaggerUrl}", urls);
                logger.LogInformation("健康檢查：{HealthUrl}/health", urls);
                logger.LogInformation("詳細健康檢查：{HealthUrl}/health/detailed", urls);
            }

            logger.LogInformation("=".PadRight(60, '='));

            return app;
        }
    }
}