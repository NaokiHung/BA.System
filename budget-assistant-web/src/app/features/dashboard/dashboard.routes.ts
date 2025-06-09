import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard.component';

/**
 * 儀表板模組路由
 * 為什麼儀表板需要獨立的路由模組？
 * 1. 未來可能會有多個儀表板頁面（例如：總覽、詳細分析）
 * 2. 支援延遲載入
 * 3. 保持架構的一致性
 */
export const dashboardRoutes: Routes = [
  { path: '', component: DashboardComponent }
];