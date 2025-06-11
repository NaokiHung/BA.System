/**
 * 開發環境設定
 * 為什麼要分離環境設定？
 * 1. 不同環境使用不同的 API 端點
 * 2. 開發環境可以啟用除錯功能
 * 3. 避免將生產環境設定意外部署到開發環境
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5091/api',  // 對應後端 API 位址
  appName: '個人理財管理系統',
  version: '1.0.0',
  enableLogging: true,                   // 開發環境啟用詳細日誌
  tokenKey: 'budget_assistant_token'     // LocalStorage 中 Token 的鍵名
};
