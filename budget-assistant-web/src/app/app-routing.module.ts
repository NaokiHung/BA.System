import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

/**
 * 主應用程式路由設定
 * 檔案路徑：budget-assistant-web/src/app/app-routing.module.ts
 * 
 * 使用 Angular 新式路由架構 (standalone components + routes)
 * 路由路徑：/user/profile
 */

const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(r => r.AUTH_ROUTES)
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.routes').then(r => r.DASHBOARD_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'expense',
    loadChildren: () => import('./features/expense/expense.routes').then(r => r.EXPENSE_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'user',
    loadChildren: () => import('./features/user/user.routes').then(r => r.USER_ROUTES),
    canActivate: [authGuard],
    title: '使用者管理'
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    enableTracing: false,
    preloadingStrategy: PreloadAllModules,
    onSameUrlNavigation: 'reload'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }