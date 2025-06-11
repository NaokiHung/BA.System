using BA.Server.API.Extensions;

// 建立 Web 應用程式建構器
var builder = WebApplication.CreateBuilder(args);

// ===========================================
// 服務容器配置
// ===========================================
// 為什麼要使用擴充方法？
// 1. 提高程式碼的可讀性和維護性
// 2. 符合單一職責原則，每個擴充方法負責一個功能領域
// 3. 方便測試和除錯
// 4. 易於擴展和修改

// 重要：服務註冊的順序很重要！
// 1. 先註冊基礎服務（日誌、資料庫）
// 2. 再註冊依賴基礎服務的應用服務（Repository、Business）
// 3. 最後註冊 Web 相關服務（API、認證、CORS）

// 1. 配置日誌服務（最基礎的服務，其他服務可能需要依賴）
builder.ConfigureLogging();

// 2. 資料庫服務配置（Repository 依賴 DbContext）
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);

// 3. Repository 依賴注入配置（依賴 DbContext 和 Logger）
builder.Services.AddRepositoryServices();

// 4. 業務服務層配置（依賴 Repository）
builder.Services.AddBusinessServices();

// 5. JWT 認證和授權配置
builder.Services.AddJwtAuthentication(builder.Configuration);

// 6. API 控制器和 CORS 配置
builder.Services.AddApiServices();
builder.Services.AddCorsServices(builder.Environment);

// 7. Swagger/OpenAPI 文檔配置
builder.Services.AddSwaggerServices();

// 8. 健康檢查配置
builder.Services.AddHealthCheckServices();

// 9. 可選：其他進階服務配置
// builder.Services.AddIntegrationServices();
// builder.Services.AddBackgroundServices();
// builder.Services.AddCustomAuthorizationPolicies();
// builder.Services.AddResponseCaching();

// ===========================================
// 建立應用程式實例
// ===========================================
var app = builder.Build();

// ===========================================
// 開發環境下的服務驗證
// ===========================================
if (app.Environment.IsDevelopment())
{
    // 驗證所有 Repository 是否能正確建立
    // 這有助於在開發階段早期發現 DI 配置問題
    var repositoryValidation = RepositoryExtensions.ValidateRepositoryConfiguration(app.Services);
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    
    if (repositoryValidation)
    {
        logger.LogInformation("✅ Repository 配置驗證成功");
    }
    else
    {
        logger.LogError("❌ Repository 配置驗證失敗");
        // 在開發環境中，如果 Repository 配置有問題，立即停止應用程式
        throw new InvalidOperationException("Repository 配置驗證失敗，請檢查 DI 設定");
    }
}

// ===========================================
// 中介軟體管線配置
// ===========================================
// 為什麼使用統一的配置方法？
// 1. 確保中介軟體的正確順序
// 2. 避免在不同環境下配置不一致
// 3. 集中管理所有中介軟體邏輯

// 配置完整的中介軟體管線
app.ConfigureMiddlewarePipeline();

// 可選：額外的中介軟體配置
// app.UseResponseCaching();
// app.UseResponseCompression();
// app.UseRateLimiting();
// app.UseCustomExceptionHandling();
// app.UseRequestLogging();

// ===========================================
// 資料庫初始化
// ===========================================
// 為什麼在這裡初始化？
// 1. 確保所有服務都已註冊完成
// 2. 應用程式啟動前確保資料庫可用
// 3. 提早發現資料庫相關問題
try
{
    app.InitializeDatabases();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "資料庫初始化失敗");
    
    // 根據環境決定是否繼續啟動
    if (app.Environment.IsDevelopment())
    {
        throw; // 開發環境中拋出異常，方便除錯
    }
    else
    {
        logger.LogWarning("生產環境中資料庫初始化失敗，應用程式將繼續啟動但可能無法正常運作");
    }
}

// ===========================================
// 應用程式啟動
// ===========================================
// 記錄啟動資訊並啟動應用程式
app.LogStartupInformation();

// 啟動應用程式
app.Run();