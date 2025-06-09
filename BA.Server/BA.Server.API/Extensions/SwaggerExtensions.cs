using Microsoft.OpenApi.Models;
using BA.Server.API.Filters;

namespace BA.Server.API.Extensions
{
    /// <summary>
    /// Swagger/OpenAPI 配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. Swagger 配置較為複雜
    /// 2. 方便根據環境調整文檔設定
    /// 3. 易於維護和擴展 API 文檔功能
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// 設定 Swagger/OpenAPI 服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            // 新增 API 探索服務
            // 為什麼需要？Swagger 需要探索可用的 API 端點
            services.AddEndpointsApiExplorer();

            // 配置 Swagger 產生器
            services.AddSwaggerGen(options =>
            {
                // 基本 API 資訊設定
                ConfigureApiInfo(options);

                // JWT 認證配置
                ConfigureJwtAuthentication(options);

                // XML 註解配置
                ConfigureXmlComments(options);

                // 自訂 Schema 篩選器
                ConfigureSchemaFilters(options);

                // 操作篩選器
                ConfigureOperationFilters(options);
            });

            return services;
        }

        /// <summary>
        /// 配置 API 基本資訊
        /// </summary>
        private static void ConfigureApiInfo(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "個人管理系統 API",
                Version = "v1.0",
                Description = @"
                    包含記帳管理和維修記錄功能的個人管理系統 API。
                    
                    ## 主要功能
                    - 使用者認證與授權
                    - 支出記錄管理
                    - 預算規劃
                    - 資料統計與分析
                    
                    ## 認證方式
                    本 API 使用 JWT (JSON Web Token) 進行認證。
                    請先呼叫登入 API 取得 Token，然後在後續請求中加入 Authorization 標頭。
                ",
                Contact = new OpenApiContact
                {
                    Name = "開發團隊",
                    Email = "developer@yourproject.com",
                    Url = new Uri("https://github.com/yourproject")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://yourproject.com/terms")
            });

            // 如果有多個版本的 API，可以新增更多文檔
            // options.SwaggerDoc("v2", new OpenApiInfo { Title = "API v2", Version = "v2.0" });
        }

        /// <summary>
        /// 配置 JWT 認證相關設定
        /// 為什麼需要？讓 Swagger UI 支援 JWT Token 測試
        /// </summary>
        private static void ConfigureJwtAuthentication(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            // 定義 JWT Bearer 認證 Schema
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
                    請輸入 JWT Token。
                    
                    格式：Bearer {your_token}
                    
                    範例：Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                "
            });

            // 設定全域安全需求
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }

        /// <summary>
        /// 配置 XML 註解文檔
        /// 為什麼使用 XML 註解？
        /// 1. 提供詳細的 API 說明
        /// 2. 包含參數和回應的描述
        /// 3. 與程式碼同步維護
        /// </summary>
        private static void ConfigureXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            // API 專案的 XML 註解
            var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            if (File.Exists(apiXmlPath))
            {
                options.IncludeXmlComments(apiXmlPath);
            }

            // 可以加入其他專案的 XML 註解
            // var coreXmlFile = "BA.Server.Core.xml";
            // var coreXmlPath = Path.Combine(AppContext.BaseDirectory, coreXmlFile);
            // if (File.Exists(coreXmlPath))
            // {
            //     options.IncludeXmlComments(coreXmlPath);
            // }
        }

        /// <summary>
        /// 配置 Schema 篩選器
        /// 用於自訂資料模型在 Swagger 中的顯示方式
        /// </summary>
        private static void ConfigureSchemaFilters(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            // 為 enum 顯示字串值
            options.SchemaFilter<EnumSchemaFilter>();

            // 範例：其他自訂 Schema 篩選器
            // options.SchemaFilter<RequiredNotNullableSchemaFilter>();
            // options.SchemaFilter<ExampleSchemaFilter>();
        }

        /// <summary>
        /// 配置操作篩選器
        /// 用於自訂 API 操作在 Swagger 中的顯示方式
        /// </summary>
        private static void ConfigureOperationFilters(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            // 範例：新增操作篩選器
            // options.OperationFilter<AuthorizeCheckOperationFilter>();
            // options.OperationFilter<FileUploadOperationFilter>();
        }

        /// <summary>
        /// 配置 Swagger UI 中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <param name="environment">環境資訊</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseSwaggerDocumentation(
            this WebApplication app, 
            IWebHostEnvironment environment)
        {
            // 只在開發和測試環境啟用 Swagger
            if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            {
                // 啟用 Swagger JSON 端點
                app.UseSwagger(options =>
                {
                    // 自訂 Swagger JSON 路徑
                    // options.RouteTemplate = "api-docs/{documentName}/swagger.json";
                });

                // 啟用 Swagger UI
                app.UseSwaggerUI(options =>
                {
                    // 基本設定
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "個人管理系統 API v1");
                    options.RoutePrefix = string.Empty; // 讓 Swagger UI 在根路徑可存取
                    options.DocumentTitle = "個人管理系統 API 文檔";

                    // 介面自訂
                    ConfigureSwaggerUI(options);

                    // 安全性設定
                    ConfigureSwaggerSecurity(options, environment);
                });
            }

            return app;
        }

        /// <summary>
        /// 配置 Swagger UI 介面
        /// </summary>
        private static void ConfigureSwaggerUI(Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions options)
        {
            // 預設展開層級
            options.DefaultModelsExpandDepth(-1); // 隱藏 Models 區段
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // 不自動展開

            // 請求/回應顯示設定
            options.DisplayRequestDuration(); // 顯示請求耗時
            options.EnableDeepLinking(); // 啟用深層連結
            options.EnableFilter(); // 啟用篩選功能
            options.ShowExtensions(); // 顯示擴充資訊

            // 自訂 CSS 和 JavaScript
            // options.InjectStylesheet("/swagger-ui/custom.css");
            // options.InjectJavascript("/swagger-ui/custom.js");

            // 設定 OAuth2 (如果使用的話)
            // options.OAuthClientId("swagger-ui");
            // options.OAuthAppName("Swagger UI");
        }

        /// <summary>
        /// 配置 Swagger 安全性設定
        /// </summary>
        private static void ConfigureSwaggerSecurity(
            Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions options,
            IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                // 開發環境可以啟用一些方便的功能
                // options.EnableValidator(); // 啟用 Schema 驗證器
            }
            else
            {
                // 非開發環境的安全性設定
                // 例如：限制存取、加入認證等
            }
        }

        /// <summary>
        /// 新增 API 版本支援到 Swagger
        /// 使用時機：當有多個 API 版本時
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合（支援鏈式呼叫）</returns>
        public static IServiceCollection AddSwaggerVersioning(this IServiceCollection services)
        {
            // 注意：需要安裝 Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer NuGet 套件
            // services.AddVersionedApiExplorer(setup =>
            // {
            //     setup.GroupNameFormat = "'v'VVV";
            //     setup.SubstituteApiVersionInUrl = true;
            // });

            return services;
        }
    }
}