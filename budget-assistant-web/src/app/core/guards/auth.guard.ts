/**
 * 檔案路徑: budget-assistant-web/src/app/core/guards/auth.guard.ts
 * 加強除錯功能的路由守衛
 */

import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

/**
 * 認證守衛 - 保護需要登入的路由
 * 為什麼需要加強除錯？
 * 1. 了解路由跳轉的原因
 * 2. 追蹤認證狀態變化
 * 3. 快速定位問題所在
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log(`🛡️ AuthGuard 檢查路由: ${state.url}`);

  return authService.isAuthenticated$.pipe(
    tap(isAuthenticated => {
      console.log(`🛡️ 認證狀態: ${isAuthenticated ? '已登入' : '未登入'}`);
    }),
    map(isAuthenticated => {
      if (isAuthenticated) {
        console.log(`✅ 允許存取: ${state.url}`);
        return true;
      } else {
        console.log(`❌ 拒絕存取，導向登入頁面: ${state.url}`);
        router.navigate(['/auth/login'], { 
          queryParams: { returnUrl: state.url }
        });
        return false;
      }
    })
  );
};

/**
 * 反向認證守衛 - 已登入使用者不能存取登入/註冊頁面
 */
export const noAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log(`🚫 NoAuthGuard 檢查路由: ${state.url}`);

  return authService.isAuthenticated$.pipe(
    tap(isAuthenticated => {
      console.log(`🚫 認證狀態: ${isAuthenticated ? '已登入' : '未登入'}`);
    }),
    map(isAuthenticated => {
      if (!isAuthenticated) {
        console.log(`✅ 允許存取認證頁面: ${state.url}`);
        return true;
      } else {
        console.log(`❌ 已登入，導向儀表板: ${state.url}`);
        router.navigate(['/dashboard']);
        return false;
      }
    })
  );
};