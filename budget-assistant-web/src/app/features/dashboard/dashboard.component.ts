// 檔案路徑: budget-assistant-web/src/app/features/dashboard/dashboard.component.ts

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
 * 儀表板組件 - 修正版本
 * 主要修正：
 * 1. 添加完整的錯誤處理和日誌
 * 2. 修正數據計算邏輯
 * 3. 移除測試功能
 * 4. 增加數據驗證
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

  // 儀表板統計數據 - 重新設計
  budgetUtilizationPercentage = 0;
  remainingDays = 0;
  currentMonth = '';
  
  // 計算後的數據
  totalSpent = 0;
  dailyAverageSpent = 0;
  dailyAverageRemaining = 0;

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
   * 為什麼需要驗證使用者？
   * 1. 確保儀表板顯示正確使用者的數據
   * 2. 處理使用者登出的情況
   */
  private getCurrentUser(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.currentUser = user;
          console.log('當前使用者:', user); // 除錯用，生產環境請移除
        },
        error: (error) => {
          console.error('取得使用者資訊失敗:', error);
          this.router.navigate(['/auth/login']);
        }
      });
  }

  /**
   * 載入儀表板資料 - 修正版本
   * 增加詳細的錯誤處理和數據驗證
   */
  private loadDashboardData(): void {
    this.isLoading = true;
    this.error = null;

    console.log('開始載入儀表板資料...'); // 除錯用

    this.expenseService.getCurrentMonthBudget()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          console.log('=== API 回傳的完整數據 ===');
          console.log('原始資料:', data);
          console.log('資料類型檢查:');
          console.log('- totalBudget:', data.totalBudget, typeof data.totalBudget);
          console.log('- remainingCash:', data.remainingCash, typeof data.remainingCash);
          console.log('- totalCashExpenses:', data.totalCashExpenses, typeof data.totalCashExpenses);
          console.log('- totalCreditCard:', data.totalCreditCard, typeof data.totalCreditCard);
          
          // 驗證數據完整性
          if (this.validateBudgetData(data)) {
            this.budgetData = data;
            this.calculateBudgetStatistics();
            this.calculateAdditionalStats();
          } else {
            this.error = '接收到的數據格式不正確';
            console.error('數據驗證失敗:', data);
          }
          
          this.isLoading = false;
        },
        error: (error) => {
          console.error('載入儀表板資料失敗:', error);
          
          // 根據錯誤類型提供不同的錯誤訊息
          if (error.status === 401) {
            this.error = '登入已過期，請重新登入';
            this.router.navigate(['/auth/login']);
          } else if (error.status === 0) {
            this.error = '無法連接到伺服器，請檢查網路連線';
          } else {
            this.error = '載入資料失敗，請稍後再試';
          }
          
          this.isLoading = false;
        }
      });
  }

  /**
   * 驗證預算數據的完整性
   * 為什麼需要驗證？
   * 1. 防止前端因為後端數據問題而出錯
   * 2. 早期發現數據問題
   * 3. 提供更好的使用者體驗
   */
  private validateBudgetData(data: MonthlyBudgetResponse): boolean {
    if (!data) {
      console.error('數據為空');
      return false;
    }

    // 檢查必要欄位
    const requiredFields = ['totalBudget', 'remainingCash', 'totalCashExpenses', 'year', 'month'];
    for (const field of requiredFields) {
      if (data[field as keyof MonthlyBudgetResponse] === undefined || data[field as keyof MonthlyBudgetResponse] === null) {
        console.error(`缺少必要欄位: ${field}`);
        return false;
      }
    }

    // 檢查數據邏輯
    if (data.totalBudget < 0 || data.remainingCash < 0 || data.totalCashExpenses < 0) {
      console.error('數據包含負值:', data);
      return false;
    }

    // 檢查計算邏輯是否正確
    const expectedRemaining = data.totalBudget - data.totalCashExpenses;
    if (Math.abs(data.remainingCash - expectedRemaining) > 0.01) { // 允許小數點誤差
      console.warn('剩餘金額計算可能有誤:', {
        expected: expectedRemaining,
        actual: data.remainingCash,
        budget: data.totalBudget,
        expenses: data.totalCashExpenses
      });
    }

    return true;
  }

  /**
   * 計算預算統計數據 - 修正版本
   * 確保計算邏輯正確
   */
  private calculateBudgetStatistics(): void {
    if (!this.budgetData) return;

    console.log('原始預算數據:', this.budgetData);

    // 計算總支出金額 - 修正：使用 totalBudget - remainingCash
    const totalBudget = Number(this.budgetData.totalBudget) || 0;
    const remainingCash = Number(this.budgetData.remainingCash) || 0;
    this.totalSpent = totalBudget - remainingCash;

    // 計算預算使用率 - 修正計算邏輯
    if (totalBudget > 0) {
      console.log('計算詳情:', {
        totalBudget: totalBudget,
        remainingCash: remainingCash,
        spent: this.totalSpent,
        calculation: (this.totalSpent / totalBudget) * 100
      });
      
      this.budgetUtilizationPercentage = Math.round((this.totalSpent / totalBudget) * 100);
      
      // 確保百分比不會超過合理範圍，但允許超過100%來顯示超支情況
      this.budgetUtilizationPercentage = Math.max(0, this.budgetUtilizationPercentage);
    } else {
      this.budgetUtilizationPercentage = 0;
    }

    // 設定當月資訊
    this.currentMonth = this.budgetData.monthName || `${this.budgetData.year}年${this.budgetData.month.toString().padStart(2, '0')}月`;

    console.log('最終計算結果:', {
      totalBudget: totalBudget,
      remainingCash: remainingCash,
      totalSpent: this.totalSpent,
      utilization: this.budgetUtilizationPercentage,
      month: this.currentMonth,
      isOverBudget: this.budgetUtilizationPercentage > 100
    });
  }

  /**
   * 計算額外的統計數據
   * 提供更多有用的財務洞察
   */
  private calculateAdditionalStats(): void {
    if (!this.budgetData) return;

    const currentDate = new Date();
    const currentDay = currentDate.getDate();
    
    // 計算平均每日支出
    if (currentDay > 0) {
      this.dailyAverageSpent = this.totalSpent / currentDay;
    }

    // 計算剩餘天數的平均每日可支出金額
    if (this.remainingDays > 0 && this.budgetData.remainingCash > 0) {
      this.dailyAverageRemaining = this.budgetData.remainingCash / this.remainingDays;
    }
  }

  /**
   * 計算本月剩餘天數
   */
  private calculateRemainingDays(): void {
    const now = new Date();
    const lastDayOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    const currentDay = now.getDate();
    this.remainingDays = lastDayOfMonth.getDate() - currentDay;
  }

  /**
   * 格式化貨幣顯示
   * 為什麼需要統一格式？
   * 1. 提供一致的使用者體驗
   * 2. 符合在地化需求
   * 3. 避免數字顯示問題
   */
  formatCurrency(amount: number): string {
    if (amount === undefined || amount === null) {
      return '$0';
    }
    
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: 'TWD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  /**
   * 刷新數據
   * 當使用者點擊重新載入按鈕時調用
   */
  refreshData(): void {
    this.loadDashboardData();
  }

  /**
   * 導航到新增支出頁面
   */
  navigateToAddExpense(): void {
    this.router.navigate(['/expense/add']);
  }

  /**
   * 導航到預算設定頁面
   */
  navigateToBudgetSetting(): void {
    this.router.navigate(['/expense/budget']);
  }

  /**
   * 導航到歷史記錄頁面
   */
  navigateToHistory(): void {
    this.router.navigate(['/expense/history']);
  }

  /**
   * 取得預算狀態顏色
   * 根據使用率返回不同的顏色主題
   */
  getBudgetStatusColor(): string {
    if (this.budgetUtilizationPercentage <= 50) return 'primary';
    if (this.budgetUtilizationPercentage <= 80) return 'accent';
    return 'warn';
  }

  /**
   * 取得預算狀態文字
   */
  getBudgetStatusText(): string {
    if (this.budgetUtilizationPercentage <= 50) return '預算充裕';
    if (this.budgetUtilizationPercentage <= 80) return '預算適中';
    if (this.budgetUtilizationPercentage <= 100) return '預算緊張';
    return '已超支';
  }

  /**
   * 取得問候語
   * 根據時間返回不同的問候語
   */
  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return '早安';
    if (hour < 18) return '午安';
    return '晚安';
  }

  /**
   * 取得當前日期
   * 格式化顯示今天的日期
   */
  getCurrentDate(): string {
    const today = new Date();
    const options: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      weekday: 'long'
    };
    return today.toLocaleDateString('zh-TW', options);
  }
}