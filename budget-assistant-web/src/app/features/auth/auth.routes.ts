import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

/**
 * 認證模組路由 - Angular 19 方式
 * 為什麼要為認證功能建立獨立路由？
 * 1. 模組化管理，職責清晰
 * 2. 支援延遲載入
 * 3. 易於維護和擴展
 */
export const authRoutes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent }
];