import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable, map } from 'rxjs';

/**
 * 反向認證守衛
 * 為什麼需要？
 * 1. 已登入的使用者不應該再次看到登入/註冊頁面
 * 2. 自動導向儀表板或主頁面
 */
@Injectable({
  providedIn: 'root'
})
export class NoAuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> {
    return this.authService.isAuthenticated$.pipe(
      map(isAuthenticated => {
        if (!isAuthenticated) {
          return true;
        } else {
          // 已登入則導向儀表板
          this.router.navigate(['/dashboard']);
          return false;
        }
      })
    );
  }
}