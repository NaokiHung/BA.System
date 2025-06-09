import { Routes } from '@angular/router';
import { authGuard, noAuthGuard } from './core/guards/auth.guard';

/**
 * Angular 19 路由配置
 * 使用延遲載入（lazy loading）提升效能
 * 為什麼使用延遲載入？
 * 1. 減少初始載入時間
 * 2. 按需載入功能模組
 * 3. 提升應用程式效能
 */
export const routes: Routes = [
  // 預設路由導向儀表板
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  
  // 認證相關路由（未登入時可存取）
  {
    path: 'auth',
    canActivate: [noAuthGuard],  // 已登入使用者不能存取
    loadChildren: () => import('./features/auth/auth.routes').then(r => r.authRoutes)
  },
  
  // 儀表板（需要登入）
  {
    path: 'dashboard',
    canActivate: [authGuard],  // 需要登入才能存取
    loadChildren: () => import('./features/dashboard/dashboard.routes').then(r => r.dashboardRoutes)
  },
  
  // 支出管理（需要登入）
  {
    path: 'expense',
    canActivate: [authGuard],
    loadChildren: () => import('./features/expense/expense.routes').then(r => r.expenseRoutes)
  },
  
  // 404 頁面
  { 
    path: '**', 
    redirectTo: '/dashboard' 
  }
];