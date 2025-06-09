import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { environment } from './environments/environment';

/**
 * Angular 19 應用程式啟動
 * 使用獨立元件架構
 */
if (environment.production) {
  // 生產環境優化
}

bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error('應用程式啟動失敗:', err));