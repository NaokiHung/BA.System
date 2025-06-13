namespace BA.Server.API.Extensions
{
    /// <summary>
    /// 中介軟體配置擴充方法
    /// 為什麼要獨立出來？
    /// 1. 中介軟體的順序非常重要，集中管理可避免錯誤
    /// 2. 方便根據環境調整中介軟體管線
    /// 3. 提高程式碼的可讀性和維護性
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 配置開發環境專用中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseDevelopmentMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // 1. 開發者例外頁面
                // 為什麼放在最前面？
                // 需要捕捉其他中介軟體產生的例外
                app.UseDeveloperExceptionPage();

                // 2. Swagger 文檔
                app.UseSwaggerDocumentation(app.Environment);

                // 3. 開發環境 CORS 政策
                app.UseCors("DevelopmentOnly");

                // 4. 可選：開發環境專用的除錯中介軟體
                // app.UseMiddleware<RequestLoggingMiddleware>();
            }

            return app;
        }

        /// <summary>
        /// 配置生產環境專用中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseProductionMiddleware(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                // 1. 全域例外處理
                // 在生產環境提供使用者友善的錯誤頁面
                app.UseExceptionHandler("/Error");

                // 2. HSTS (HTTP Strict Transport Security)
                // 強制使用 HTTPS 連接
                app.UseHsts();

                // 3. 生產環境 CORS 政策
                app.UseCors("AllowFrontendApps");

                // 4. 可選：安全性標頭中介軟體
                // app.UseMiddleware<SecurityHeadersMiddleware>();
            }

            return app;
        }

        /// <summary>
        /// 配置安全性相關中介軟體
        /// 為什麼要獨立出來？
        /// 1. 安全性配置容易出錯，集中管理更安全
        /// 2. 順序非常重要，錯誤的順序會導致安全漏洞
        /// 3. 方便進行安全性稽核
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseSecurityMiddleware(this WebApplication app)
        {
            // 1. HTTPS 重新導向
            // 為什麼要放在前面？
            // 確保所有後續操作都在安全連接上進行
            app.UseHttpsRedirection();

            // 2. 路由中介軟體
            // 必須在認證和授權之前
            app.UseRouting();

            // 3. 認證中介軟體
            // 為什麼要在授權之前？
            // 需要先確認使用者身分，才能進行權限檢查
            app.UseAuthentication();

            // 4. 授權中介軟體
            // 根據使用者身分進行權限檢查
            app.UseAuthorization();

            return app;
        }

        /// <summary>
        /// 配置靜態檔案中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseStaticFileMiddleware(this WebApplication app)
        {
            // 靜態檔案服務
            // 通常用於提供前端應用程式的檔案
            app.UseStaticFiles();

            // 可選：配置特定的靜態檔案路徑
            // app.UseStaticFiles(new StaticFileOptions
            // {
            //     FileProvider = new PhysicalFileProvider(
            //         Path.Combine(app.Environment.ContentRootPath, "uploads")),
            //     RequestPath = "/uploads"
            // });

            return app;
        }

        /// <summary>
        /// 配置 API 端點對應
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseApiEndpoints(this WebApplication app)
        {
            // API 控制器路由對應
            app.MapControllers();

            // 健康檢查端點
            app.UseHealthCheckEndpoints();

            // 可選：自訂路由端點
            // app.MapGet("/", () => "個人管理系統 API 運行中");
            // app.MapGet("/version", () => new { Version = "1.0.0", BuildDate = DateTime.UtcNow });

            return app;
        }

        /// <summary>
        /// 配置完整的中介軟體管線
        /// 這是主要的入口方法，按正確順序配置所有中介軟體
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
        {
            // 中介軟體的順序非常重要！
            // 以下是推薦的標準順序：

            // 1. 環境專用中介軟體（必須最先）
            app.UseDevelopmentMiddleware();
            app.UseProductionMiddleware();

            // 2. 安全性中介軟體
            app.UseSecurityMiddleware();

            // 3. 靜態檔案服務
            app.UseStaticFileMiddleware();

            // 4. 路由端點對應（必須最後）
            app.UseApiEndpoints();

            return app;
        }

        /// <summary>
        /// 新增回應快取中介軟體
        /// 使用時機：當需要快取 API 回應時
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseResponseCaching(this WebApplication app)
        {
            // 注意：必須在 UseRouting() 之後，MapControllers() 之前使用
            app.UseResponseCaching();

            return app;
        }

        /// <summary>
        /// 新增回應壓縮中介軟體
        /// 使用時機：當需要壓縮 HTTP 回應以節省頻寬時
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseResponseCompression(this WebApplication app)
        {
            // 注意：必須在其他中介軟體之前使用
            app.UseResponseCompression();

            return app;
        }

        /// <summary>
        /// 新增限流中介軟體
        /// 使用時機：當需要限制 API 呼叫頻率時
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseRateLimiting(this WebApplication app)
        {
            // 注意：需要安裝適當的限流套件
            // app.UseIpRateLimiting();

            return app;
        }

        /// <summary>
        /// 新增自訂例外處理中介軟體
        /// 使用時機：當需要統一處理和記錄例外時
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseCustomExceptionHandling(this WebApplication app)
        {
            // 範例：自訂例外處理中介軟體
            // app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            return app;
        }

        /// <summary>
        /// 新增請求記錄中介軟體
        /// 使用時機：當需要詳細記錄 HTTP 請求資訊時
        /// </summary>
        /// <param name="app">Web 應用程式</param>
        /// <returns>Web 應用程式（支援鏈式呼叫）</returns>
        public static WebApplication UseRequestLogging(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // 開發環境才啟用詳細的請求記錄
                // app.UseMiddleware<RequestResponseLoggingMiddleware>();
            }

            return app;
        }
    }
}