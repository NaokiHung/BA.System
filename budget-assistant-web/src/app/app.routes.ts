/**
 * 檔案路徑: budget-assistant-web/src/app/app.routes.ts
 * 暫時的路由配置 - 用於除錯
 */

import { Routes } from '@angular/router';
import { authGuard, noAuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // 暫時改為導向登入頁面，避免認證問題
  { 
    path: '', 
    redirectTo: '/auth/login', // 暫時改成登入頁面
    pathMatch: 'full' 
  },
  
  // 認證相關路由（未登入時可存取）
  {
    path: 'auth',
    canActivate: [noAuthGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(r => r.authRoutes)
  },
  
  // 儀表板（暫時移除路由守衛進行測試）
  {
    path: 'dashboard',
    // canActivate: [authGuard], // 暫時註解掉
    loadChildren: () => import('./features/dashboard/dashboard.routes').then(r => r.dashboardRoutes)
  },
  
  // 支出管理
  {
    path: 'expense',
    canActivate: [authGuard],
    loadChildren: () => import('./features/expense/expense.routes').then(r => r.expenseRoutes)
  },
  
  // 404 頁面
  { 
    path: '**', 
    redirectTo: '/auth/login' // 暫時改成登入頁面
  }
];