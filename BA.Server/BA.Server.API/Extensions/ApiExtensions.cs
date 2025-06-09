using Microsoft.AspNetCore.Mvc;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// API 控制器和 CORS 配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. API 相關配置邏輯集中管理
    /// 2. CORS 設定較為複雜，需要根據環境調整
    /// 3. 模型驗證邏輯可以統一處理
    /// </summary>
    public static class ApiExtensions
    {
        /// <summary>
        /// 設定 API 控制器相關服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // 配置控制器服務
            services.AddControllers(options =>
            {
                // 清除預設的模型驗證提供者
                // 為什麼要清除？可以提供更一致的驗證行為
                options.ModelValidatorProviders.Clear();

                // 未來可以新增全域過濾器
                // options.Filters.Add<GlobalExceptionFilter>();
                // options.Filters.Add<GlobalActionFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // 自訂模型驗證錯誤回應格式
                // 為什麼要自訂？提供統一的錯誤回應格式
                options.InvalidModelStateResponseFactory = CreateModelValidationErrorResponse;
            });

            return services;
        }

        /// <summary>
        /// 設定 CORS 政策
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="environment">環境資訊</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddCorsServices(
            this IServiceCollection services, 
            IWebHostEnvironment environment)
        {
            services.AddCors(options =>
            {
                // 生產環境 CORS 政策
                options.AddPolicy("AllowFrontendApps", policy =>
                {
                    // 為什麼要限制特定來源？
                    // 1. 提高安全性
                    // 2. 避免 CSRF 攻擊
                    // 3. 符合最小權限原則
                    policy.WithOrigins(
                            "http://localhost:4200",    // Angular 開發伺服器
                            "http://localhost:3000",    // React 開發伺服器
                            "https://localhost:8586"    // 本地 HTTPS
                        )
                        .AllowAnyMethod()           // 允許所有 HTTP 方法
                        .AllowAnyHeader()           // 允許所有標頭
                        .AllowCredentials();        // 允許認證資訊（Cookies, Authorization headers）
                });

                // 開發環境專用政策
                if (environment.IsDevelopment())
                {
                    options.AddPolicy("DevelopmentOnly", policy =>
                    {
                        // 注意：此政策僅供開發使用，不應在生產環境啟用
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                        // 注意：AllowAnyOrigin() 和 AllowCredentials() 不能同時使用
                    });
                }

                // 範例：特定 API 的 CORS 政策
                // options.AddPolicy("ApiOnly", policy =>
                // {
                //     policy.WithOrigins("https://your-spa-domain.com")
                //           .WithMethods("GET", "POST", "PUT", "DELETE")
                //           .WithHeaders("Content-Type", "Authorization");
                // });
            });

            return services;
        }

        /// <summary>
        /// 建立模型驗證錯誤回應
        /// 為什麼要自訂？
        /// 1. 提供統一的錯誤回應格式
        /// 2. 方便前端處理錯誤訊息
        /// 3. 符合 RESTful API 的最佳實踐
        /// </summary>
        private static IActionResult CreateModelValidationErrorResponse(ActionContext context)
        {
            // 收集所有驗證錯誤訊息
            var errors = context.ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            // 建立統一的錯誤回應格式
            var result = new
            {
                Success = false,
                Message = "輸入資料驗證失敗",
                Errors = errors,
                // 在開發環境可以提供更多除錯資訊
                Details = context.HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment() ? context.ModelState : null
            };

            return new BadRequestObjectResult(result);
        }

        /// <summary>
        /// 新增 API 版本控制支援
        /// 使用時機：當 API 需要版本管理時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddApiVersioning(this IServiceCollection services)
        {
            // 注意：需要安裝 Microsoft.AspNetCore.Mvc.Versioning NuGet 套件
            // services.AddApiVersioning(options =>
            // {
            //     options.DefaultApiVersion = new ApiVersion(1, 0);
            //     options.AssumeDefaultVersionWhenUnspecified = true;
            //     options.ApiVersionReader = ApiVersionReader.Combine(
            //         new QueryStringApiVersionReader("version"),
            //         new HeaderApiVersionReader("X-Version"),
            //         new UrlSegmentApiVersionReader()
            //     );
            // });

            return services;
        }

        /// <summary>
        /// 新增 API 限流支援
        /// 使用時機：當需要限制 API 呼叫頻率時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            // 注意：需要安裝適當的限流套件
            // services.AddMemoryCache();
            // services.Configure<IpRateLimitOptions>(options =>
            // {
            //     options.GeneralRules = new List<RateLimitRule>
            //     {
            //         new RateLimitRule
            //         {
            //             Endpoint = "*",
            //             Limit = 100,
            //             Period = "1m"
            //         }
            //     };
            // });

            return services;
        }

        /// <summary>
        /// 新增回應快取支援
        /// 使用時機：當 API 回應需要快取時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddResponseCaching(this IServiceCollection services)
        {
            services.AddResponseCaching(options =>
            {
                // 設定回應快取的最大大小
                options.MaximumBodySize = 1024 * 1024; // 1MB
                
                // 設定是否使用區分大小寫的路徑
                options.UseCaseSensitivePaths = false;
            });

            return services;
        }
    }
}