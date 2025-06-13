using BA.Server.API.Extensions;

// 建立 Web 應用程式建構器
var builder = WebApplication.CreateBuilder(args);

// ===========================================
// 服務容器配置（簡化版本）
// ===========================================

// 1. 配置日誌服務
builder.ConfigureLogging();

// 2. 資料庫服務配置
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);

// 3. Repository 依賴注入配置（簡化版本）
builder.Services.AddRepositoryServices();

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

// ===========================================
// 建立應用程式實例
// ===========================================
var app = builder.Build();

// ===========================================
// 中介軟體管線配置
// ===========================================
app.ConfigureMiddlewarePipeline();

// ===========================================
// 資料庫初始化
// ===========================================
try
{
    app.InitializeDatabases();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "資料庫初始化失敗");
    
    if (app.Environment.IsDevelopment())
    {
        throw; // 開發環境中拋出異常，方便除錯
    }
}

// ===========================================
// 應用程式啟動
// ===========================================
app.LogStartupInformation();
app.Run();