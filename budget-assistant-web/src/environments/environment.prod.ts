/**
 * 生產環境設定
 */
export const environment = {
  production: true,
  apiUrl: 'https://your-api-domain.com/api',  // 生產環境 API 位址
  appName: '個人理財管理系統',
  version: '1.0.0',
  enableLogging: false,                        // 生產環境關閉詳細日誌
  tokenKey: 'budget_assistant_token'
};