import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * HTTP 認證攔截器
 * 為什麼需要攔截器？
 * 1. 自動為所有 HTTP 請求添加 Authorization 標頭
 * 2. 統一處理認證失敗的情況
 * 3. 避免在每個服務中重複添加 Token
 * 4. 自動處理 401 錯誤並導向登入頁面
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token && authService.isTokenValid()) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error) => {
      if (error.status === 401) {
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};