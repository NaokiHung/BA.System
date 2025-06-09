import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { Observable, Subject, takeUntil } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../core/services/auth.service';
import { User } from '../core/models/auth.models';

/**
 * 主要版面配置組件 - Angular 19 獨立元件
 * 為什麼使用響應式設計？
 * 1. 根據認證狀態動態調整版面
 * 2. 支援不同螢幕尺寸的響應式布局
 * 3. 統一管理導航和側邊欄
 */
@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private destroy$ = new Subject<void>();
  
  isAuthenticated$: Observable<boolean>;
  currentUser$: Observable<User | null>;
  sidenavOpened = false;

  constructor() {
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // 監聽認證狀態變化
    this.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        if (!isAuth) {
          this.sidenavOpened = false; // 未登入時關閉側邊欄
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 切換側邊欄狀態
   */
  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }

  /**
   * 登出處理
   */
  logout(): void {
    this.authService.logout();
  }

  /**
   * 導航到指定路由
   */
  navigateTo(route: string): void {
    this.router.navigate([route]);
    this.sidenavOpened = false; // 手機版導航後關閉側邊欄
  }
}