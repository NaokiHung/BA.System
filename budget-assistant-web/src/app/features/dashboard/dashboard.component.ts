import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExpenseService } from '../../core/services/expense.service';
import { AuthService } from '../../core/services/auth.service';
import { MonthlyBudgetResponse, User, SetBudgetRequest } from '../../core/models/expense.models';

/**
 * 儀表板組件 - 增強版本，支援直接編輯預算
 * 為什麼在儀表板加入編輯功能？
 * 1. 提升使用者體驗，減少頁面跳轉
 * 2. 快速調整預算，更符合實際使用場景
 * 3. 即時反映變更，保持資料一致性
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private expenseService = inject(ExpenseService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<void>();
  
  currentUser: User | null = null;
  budgetData: MonthlyBudgetResponse | null = null;
  isLoading = true;
  error: string | null = null;

  // 預算編輯相關狀態
  isEditingBudget = false;
  budgetEditForm!: FormGroup;
  isSavingBudget = false;

  // 儀表板統計數據
  budgetUtilizationPercentage = 0;
  remainingDays = 0;
  currentMonth = '';

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadDashboardData();
    this.calculateRemainingDays();
    this.initBudgetEditForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 初始化預算編輯表單
   * 為什麼要初始化空表單？
   * 避免在編輯模式切換時重新建立表單，保持表單狀態
   */
  private initBudgetEditForm(): void {
    this.budgetEditForm = this.fb.group({
      amount: ['', [
        Validators.required,
        Validators.min(1),
        Validators.max(9999999.99)
      ]]
    });
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
   * 計算當月剩餘天數
   */
  private calculateRemainingDays(): void {
    const now = new Date();
    const lastDayOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    this.remainingDays = lastDayOfMonth.getDate() - now.getDate();
  }

  /**
   * 開始編輯預算
   * 為什麼使用內聯編輯？
   * 1. 更直觀的使用者體驗
   * 2. 減少頁面跳轉，提升效率
   * 3. 即時預覽變更效果
   */
  startEditBudget(): void {
    if (this.budgetData) {
      this.isEditingBudget = true;
      this.budgetEditForm.patchValue({
        amount: this.budgetData.totalBudget
      });
    }
  }

  /**
   * 取消編輯預算
   */
  cancelEditBudget(): void {
    this.isEditingBudget = false;
    this.budgetEditForm.reset();
  }

  /**
   * 儲存預算變更
   * 為什麼要在儲存後重新載入資料？
   * 確保顯示的資料與後端狀態完全一致
   */
  saveBudgetChanges(): void {
    if (this.budgetEditForm.valid && !this.isSavingBudget && this.budgetData) {
      this.isSavingBudget = true;

      const request: SetBudgetRequest = {
        amount: this.budgetEditForm.value.amount,
        year: this.budgetData.year,
        month: this.budgetData.month
      };

      this.expenseService.setMonthlyBudget(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.snackBar.open('預算更新成功！', '關閉', {
                duration: 3000,
                panelClass: ['success-snackbar']
              });
              this.isEditingBudget = false;
              this.loadDashboardData(); // 重新載入資料以確保一致性
            } else {
              this.snackBar.open(response.message, '關閉', {
                duration: 5000,
                panelClass: ['error-snackbar']
              });
            }
            this.isSavingBudget = false;
          },
          error: (error) => {
            console.error('更新預算失敗:', error);
            this.snackBar.open('更新預算失敗，請稍後再試', '關閉', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
            this.isSavingBudget = false;
          }
        });
    }
  }

  /**
   * 取得預算編輯欄位的錯誤訊息
   */
  getBudgetEditErrorMessage(): string {
    const amountField = this.budgetEditForm.get('amount');
    
    if (amountField?.hasError('required')) {
      return '預算金額不能為空';
    }
    
    if (amountField?.hasError('min')) {
      return '預算金額必須大於 0';
    }
    
    if (amountField?.hasError('max')) {
      return '預算金額不能超過 9,999,999.99';
    }
    
    return '';
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
   * 取得預算使用狀態的顏色
   */
  getBudgetProgressColor(): string {
    if (this.budgetUtilizationPercentage < 70) return 'primary';
    if (this.budgetUtilizationPercentage < 90) return 'accent';
    return 'warn';
  }

  /**
   * 取得預算狀態顏色（模板中使用的方法）
   */
  getBudgetStatusColor(): 'primary' | 'accent' | 'warn' {
    if (this.budgetUtilizationPercentage < 70) return 'primary';
    if (this.budgetUtilizationPercentage < 90) return 'accent';
    return 'warn';
  }

  /**
   * 導航功能
   */
  navigateToAddExpense(): void {
    this.router.navigate(['/expense/add']);
  }

  navigateToExpense(): void {
    this.router.navigate(['/expense/add']);
  }

  navigateToBudgetSetting(): void {
    this.router.navigate(['/expense/budget']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/expense/budget']);
  }

  navigateToExpenseHistory(): void {
    this.router.navigate(['/expense/history']);
  }

  navigateToHistory(): void {
    this.router.navigate(['/expense/history']);
  }

  refreshData(): void {
    this.loadDashboardData();
  }
}