import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';

/**
 * 核心模組
 * 為什麼要建立核心模組？
 * 1. 確保核心服務只被載入一次（單例模式）
 * 2. 集中管理全域服務和攔截器
 * 3. 防止重複匯入造成的問題
 */
@NgModule({
  declarations: [],
  imports: [CommonModule],
  providers: [
    // 註冊 HTTP 攔截器
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true  // 允許多個攔截器
    }
  ]
})
export class CoreModule {
  /**
   * 防止核心模組被重複匯入
   * 為什麼要這樣做？確保核心服務的單例性
   */
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule 已經載入，它只能在 AppModule 中匯入一次。');
    }
  }
}