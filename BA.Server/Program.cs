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

// 1. 配置日誌服務
builder.ConfigureLogging();

// 2. 資料庫服務配置
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);

// 3. Repository 依賴注入配置
builder.Services.AddRepositories();

// 4. 業務服務層配置
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

// 可選：其他進階服務配置
// builder.Services.AddIntegrationServices();
// builder.Services.AddBackgroundServices();
// builder.Services.AddCustomAuthorizationPolicies();
// builder.Services.AddResponseCaching();

// ===========================================
// 建立應用程式實例
// ===========================================
var app = builder.Build();

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
app.InitializeDatabases();

// ===========================================
// 應用程式啟動
// ===========================================
// 記錄啟動資訊並啟動應用程式
app.LogStartupInformation();

// 啟動應用程式
app.Run();