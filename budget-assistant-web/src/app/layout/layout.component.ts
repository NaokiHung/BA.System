/**
 * 檔案路徑: budget-assistant-web/src/app/layout/layout.component.ts
 * 簡化的 Layout 組件，用於除錯
 */

import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AuthService } from '../core/services/auth.service';
import { User } from '../core/models/auth.models';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit, OnDestroy {
  authService = inject(AuthService); // 改為 public
  router = inject(Router); // 改為 public
  private destroy$ = new Subject<void>();
  
  isAuthenticated$: Observable<boolean>;
  currentUser$: Observable<User | null>;

  constructor() {
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.currentUser$ = this.authService.currentUser$;
    console.log('🏗️ Layout 組件初始化');
  }

  ngOnInit(): void {
    console.log('🏗️ Layout ngOnInit');
    
    // 監聽認證狀態變化
    this.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        console.log('🔐 認證狀態變化:', isAuth);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 導航到指定路由
   */
  navigateTo(route: string): void {
    console.log('🧭 導航到:', route);
    this.router.navigate([route]);
  }

  /**
   * 清除本地儲存
   */
  clearStorage(): void {
    console.log('🗑️ 清除本地儲存');
    localStorage.clear();
    sessionStorage.clear();
    window.location.reload();
  }
}