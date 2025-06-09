import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ExpenseService } from '../../core/services/expense.service';
import { AuthService } from '../../core/services/auth.service';
import { MonthlyBudgetResponse, User } from '../../core/models/expense.models';

/**
 * 儀表板組件 - Angular 19 獨立元件
 * 為什麼需要儀表板？
 * 1. 提供系統概覽和關鍵指標
 * 2. 作為使用者的主要導航起點
 * 3. 快速存取常用功能
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private expenseService = inject(ExpenseService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private destroy$ = new Subject<void>();
  
  currentUser: User | null = null;
  budgetData: MonthlyBudgetResponse | null = null;
  isLoading = true;
  error: string | null = null;

  // 儀表板統計數據
  budgetUtilizationPercentage = 0;
  remainingDays = 0;
  currentMonth = '';

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadDashboardData();
    this.calculateRemainingDays();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 取得當前使用者資訊
   */
  private getCurrentUser(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });
  }

  /**
   * 載入儀表板資料
   */
  private loadDashboardData(): void {
    this.isLoading = true;
    this.error = null;

    this.expenseService.getCurrentMonthBudget()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.budgetData = data;
          this.calculateBudgetStatistics();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('載入儀表板資料失敗:', error);
          this.error = '載入資料失敗，請稍後再試';
          this.isLoading = false;
        }
      });
  }

  /**
   * 計算預算統計數據
   */
  private calculateBudgetStatistics(): void {
    if (this.budgetData) {
      // 計算預算使用率
      if (this.budgetData.totalBudget > 0) {
        const usedAmount = this.budgetData.totalBudget - this.budgetData.remainingCash;
        this.budgetUtilizationPercentage = Math.round((usedAmount / this.budgetData.totalBudget) * 100);
      }

      this.currentMonth = this.budgetData.monthName;
    }
  }

  /**
   * 計算本月剩餘天數
   */
  private calculateRemainingDays(): void {
    const now = new Date();
    const lastDayOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    this.remainingDays = Math.max(0, lastDayOfMonth.getDate() - now.getDate());
  }

  /**
   * 重新載入資料
   */
  refreshData(): void {
    this.loadDashboardData();
  }

  /**
   * 格式化金額顯示
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: 'TWD',
      minimumFractionDigits: 0
    }).format(amount);
  }

  /**
   * 取得預算狀態的顏色
   */
  getBudgetStatusColor(): string {
    if (this.budgetUtilizationPercentage <= 60) return 'primary';
    if (this.budgetUtilizationPercentage <= 80) return 'accent';
    return 'warn';
  }

  /**
   * 導航方法
   */
  navigateToExpense(): void {
    this.router.navigate(['/expense/add']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/expense/budget']);
  }

  navigateToHistory(): void {
    this.router.navigate(['/expense/history']);
  }
}