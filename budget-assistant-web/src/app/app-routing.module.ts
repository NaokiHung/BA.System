import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { NoAuthGuard } from './core/guards/no-auth.guard';

/**
 * 應用程式路由配置
 * 為什麼使用延遲載入（lazy loading）？
 * 1. 減少初始載入時間
 * 2. 按需載入功能模組
 * 3. 提升應用程式效能
 */
const routes: Routes = [
  // 預設路由導向儀表板
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  
  // 認證相關路由（未登入時可存取）
  {
    path: 'auth',
    canActivate: [NoAuthGuard],  // 已登入使用者不能存取
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  
  // 儀表板（需要登入）
  {
    path: 'dashboard',
    canActivate: [AuthGuard],  // 需要登入才能存取
    loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  
  // 支出管理（需要登入）
  {
    path: 'expense',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/expense/expense.module').then(m => m.ExpenseModule)
  },
  
  // 404 頁面
  { 
    path: '**', 
    redirectTo: '/dashboard' 
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}