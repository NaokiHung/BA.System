using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// JWT 認證和授權配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. 認證邏輯相對複雜，獨立管理更清晰
    /// 2. 方便未來調整認證策略
    /// 3. 支援多種認證方式的擴展
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// 設定 JWT 認證服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置物件</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // 取得 JWT 設定
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            // 驗證必要的設定值
            ValidateJwtSettings(secretKey, jwtSettings);

            // 設定 JWT 認證
            ConfigureJwtAuthentication(services, secretKey!, jwtSettings);

            // 設定授權政策
            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// 驗證 JWT 設定的完整性
        /// 為什麼要獨立驗證？
        /// 1. 提早發現設定錯誤
        /// 2. 提供清楚的錯誤訊息
        /// 3. 避免應用程式在執行時才發現問題
        /// </summary>
        private static void ValidateJwtSettings(string? secretKey, IConfigurationSection jwtSettings)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey 不能為空，請檢查 appsettings.json 中的 JwtSettings:SecretKey 設定");
            }

            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(issuer))
            {
                throw new InvalidOperationException(
                    "JWT Issuer 不能為空，請檢查 appsettings.json 中的 JwtSettings:Issuer 設定");
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException(
                    "JWT Audience 不能為空，請檢查 appsettings.json 中的 JwtSettings:Audience 設定");
            }
        }

        /// <summary>
        /// 配置 JWT 認證服務
        /// </summary>
        private static void ConfigureJwtAuthentication(
            IServiceCollection services, 
            string secretKey, 
            IConfigurationSection jwtSettings)
        {
            services.AddAuthentication(options =>
            {
                // 設定預設的認證和挑戰方案
                // 為什麼選擇 JwtBearer？
                // 1. 適合 Web API
                // 2. 無狀態認證
                // 3. 可攜帶使用者資訊
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // 設定 Token 驗證參數
                options.TokenValidationParameters = CreateTokenValidationParameters(secretKey, jwtSettings);

                // 設定 JWT 相關事件處理
                options.Events = CreateJwtBearerEvents();
            });
        }

        /// <summary>
        /// 建立 Token 驗證參數
        /// 為什麼要獨立出來？
        /// 1. 參數設定較複雜，獨立管理更清楚
        /// 2. 方便測試和維護
        /// 3. 可重複使用於其他地方
        /// </summary>
        private static TokenValidationParameters CreateTokenValidationParameters(
            string secretKey, 
            IConfigurationSection jwtSettings)
        {
            return new TokenValidationParameters
            {
                // 驗證簽名金鑰
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                // 驗證發行者 (Issuer)
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],

                // 驗證接收者 (Audience)
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],

                // 驗證 Token 有效期
                ValidateLifetime = true,
                RequireExpirationTime = true,

                // 消除時間偏差
                // 為什麼設為 Zero？
                // 預設有 5 分鐘的時間偏差容忍，設為 Zero 可以更精確控制過期時間
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// 建立 JWT Bearer 事件處理器
        /// 為什麼需要事件處理？
        /// 1. 記錄認證過程中的重要事件
        /// 2. 協助除錯和監控
        /// 3. 可以在認證失敗時執行額外邏輯
        /// </summary>
        private static JwtBearerEvents CreateJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                // 認證失敗事件
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    
                    logger.LogWarning("JWT 認證失敗: {Error}", context.Exception.Message);
                    
                    // 在開發環境提供更詳細的錯誤資訊
                    if (context.HttpContext.RequestServices
                        .GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                    {
                        logger.LogDebug("JWT 認證失敗詳細資訊: {Exception}", context.Exception);
                    }

                    return Task.CompletedTask;
                },

                // Token 驗證成功事件
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    
                    var userId = context.Principal?.Identity?.Name;
                    logger.LogInformation("JWT Token 驗證成功，使用者: {UserId}", userId);

                    return Task.CompletedTask;
                },

                // 挑戰事件（當認證失敗時觸發）
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    
                    logger.LogWarning("JWT 認證挑戰: {Error}", context.Error);

                    return Task.CompletedTask;
                }
            };
        }

        /// <summary>
        /// 新增自訂授權政策
        /// 使用時機：當您需要更細緻的權限控制時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddCustomAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // 範例：要求使用者必須有特定的 Claim
                // options.AddPolicy("RequireAdminRole", policy =>
                //     policy.RequireClaim("role", "admin"));

                // 範例：要求使用者必須年滿 18 歲
                // options.AddPolicy("RequireAdult", policy =>
                //     policy.RequireAssertion(context =>
                //         context.User.HasClaim("age", age => int.Parse(age) >= 18)));

                // 範例：結合多個需求
                // options.AddPolicy("RequireManagerAccess", policy =>
                //     policy.RequireAuthenticatedUser()
                //           .RequireClaim("department", "management")
                //           .RequireClaim("level", "manager", "director"));
            });

            return services;
        }
    }
}