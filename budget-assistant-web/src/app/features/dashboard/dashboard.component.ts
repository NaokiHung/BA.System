/**
 * 檔案路徑: budget-assistant-web/src/app/features/dashboard/dashboard.component.ts
 * 修正 TypeScript 錯誤的 Dashboard 組件
 */

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
import { MonthlyBudgetResponse } from '../../core/models/expense.models';
import { User } from '../../core/models/auth.models'; // 從正確的模型導入 User

/**
 * 儀表板組件 - 修正版本
 * 修正問題：
 * 1. totalExpenses → totalCashExpenses
 * 2. authService 改為 public
 * 3. 完整的錯誤處理
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
  public authService = inject(AuthService); // 改為 public 以便在模板中使用
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

  constructor() {
    console.log('📊 Dashboard 組件建構中...');
  }

  ngOnInit(): void {
    console.log('📊 Dashboard 組件初始化開始');
    
    try {
      this.getCurrentUser();
      this.loadDashboardData();
      this.calculateRemainingDays();
      
      console.log('📊 Dashboard 組件初始化完成');
    } catch (error) {
      console.error('❌ Dashboard 組件初始化失敗:', error);
      this.error = '組件初始化失敗';
      this.isLoading = false;
    }
  }

  ngOnDestroy(): void {
    console.log('📊 Dashboard 組件銷毀');
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 取得當前使用者資訊
   */
  private getCurrentUser(): void {
    console.log('👤 正在取得使用者資訊...');
    
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          console.log('👤 使用者資訊:', user);
          this.currentUser = user;
        },
        error: (error) => {
          console.error('❌ 取得使用者資訊失敗:', error);
        }
      });
  }

  /**
   * 載入儀表板資料
   */
  private loadDashboardData(): void {
    console.log('📈 正在載入儀表板資料...');
    this.isLoading = true;
    this.error = null;

    // 暫時使用假資料進行測試
    setTimeout(() => {
      console.log('📈 使用測試資料');
      this.budgetData = {
        month: new Date().getMonth() + 1,
        year: new Date().getFullYear(),
        monthName: `${new Date().getFullYear()}年${new Date().getMonth() + 1}月`,
        totalBudget: 30000,
        remainingCash: 15000,
        totalCashExpenses: 15000, // 修正：使用正確的屬性名稱
        totalSubscriptions: 2500,
        totalCreditCard: 8000,
        combinedCreditTotal: 10500
      };
      this.calculateBudgetStatistics();
      this.isLoading = false;
    }, 1000);

    // 真實的 API 呼叫（先註解掉進行測試）
    /*
    this.expenseService.getCurrentMonthBudget()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          console.log('📈 載入儀表板資料成功:', data);
          this.budgetData = data;
          this.calculateBudgetStatistics();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ 載入儀表板資料失敗:', error);
          this.error = '載入資料失敗，請稍後再試';
          this.isLoading = false;
        }
      });
    */
  }

  /**
   * 計算預算統計數據
   */
  private calculateBudgetStatistics(): void {
    console.log('🧮 正在計算預算統計...');
    
    if (this.budgetData) {
      // 計算預算使用率
      if (this.budgetData.totalBudget > 0) {
        const usedAmount = this.budgetData.totalBudget - this.budgetData.remainingCash;
        this.budgetUtilizationPercentage = Math.round((usedAmount / this.budgetData.totalBudget) * 100);
      }

      this.currentMonth = this.budgetData.monthName;
      
      console.log('🧮 預算統計完成:', {
        使用率: this.budgetUtilizationPercentage + '%',
        當前月份: this.currentMonth
      });
    }
  }

  /**
   * 計算剩餘天數
   */
  private calculateRemainingDays(): void {
    const today = new Date();
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    this.remainingDays = lastDay.getDate() - today.getDate();
    
    console.log('📅 本月剩餘天數:', this.remainingDays);
  }

  /**
   * 導航到支出頁面
   */
  navigateToExpenses(): void {
    console.log('🧾 導航到支出頁面');
    this.router.navigate(['/expense']);
  }

  /**
   * 重新載入資料
   */
  refreshData(): void {
    console.log('🔄 重新載入資料');
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