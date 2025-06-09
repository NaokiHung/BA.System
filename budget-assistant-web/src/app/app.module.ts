import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { LayoutModule } from './layout/layout.module';

import { AppComponent } from './app.component';

/**
 * 應用程式根模組
 * 為什麼這樣組織模組？
 * 1. CoreModule：全域單例服務，只載入一次
 * 2. SharedModule：共用組件和模組，可重複使用
 * 3. LayoutModule：版面配置相關組件
 * 4. 功能模組：延遲載入，按需載入
 */
@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,  // Angular Material 需要
    HttpClientModule,         // HTTP 客戶端
    
    AppRoutingModule,
    CoreModule,               // 核心模組（只匯入一次）
    SharedModule,             // 共用模組
    LayoutModule              // 版面配置模組
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}