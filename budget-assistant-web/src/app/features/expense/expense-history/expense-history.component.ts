import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExpenseService } from '../../../core/services/expense.service';
import { ExpenseHistory } from '../../../core/models/expense.models';

/**
 * 支出記錄組件 - Angular 19 獨立元件
 * 為什麼需要支出記錄功能？
 * 1. 提供歷史支出查詢
 * 2. 幫助使用者分析消費習慣
 * 3. 支援不同月份的記錄檢視
 */
@Component({
  selector: 'app-expense-history',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './expense-history.component.html',
  styleUrls: ['./expense-history.component.scss']
})
export class ExpenseHistoryComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private expenseService = inject(ExpenseService);
  private snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<void>();

  searchForm!: FormGroup;
  expenseHistory: ExpenseHistory[] = [];
  isLoading = false;
  error: string | null = null;

  // 表格顯示的欄位
  displayedColumns: string[] = ['date', 'description', 'category', 'amount'];

  // 年份和月份選項
  years: number[] = [];
  months = [
    { value: 1, name: '一月' },
    { value: 2, name: '二月' },
    { value: 3, name: '三月' },
    { value: 4, name: '四月' },
    { value: 5, name: '五月' },
    { value: 6, name: '六月' },
    { value: 7, name: '七月' },
    { value: 8, name: '八月' },
    { value: 9, name: '九月' },
    { value: 10, name: '十月' },
    { value: 11, name: '十一月' },
    { value: 12, name: '十二月' }
  ];

  ngOnInit(): void {
    this.generateYearOptions();
    this.createForm();
    this.loadCurrentMonthHistory();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 產生年份選項
   */
  private generateYearOptions(): void {
    const currentYear = new Date().getFullYear();
    for (let year = currentYear - 2; year <= currentYear + 1; year++) {
      this.years.push(year);
    }
  }

  /**
   * 建立搜尋表單
   */
  private createForm(): void {
    const now = new Date();
    
    this.searchForm = this.fb.group({
      year: [now.getFullYear(), [Validators.required]],
      month: [now.getMonth() + 1, [Validators.required]]
    });
  }

  /**
   * 載入當月支出記錄
   */
  private loadCurrentMonthHistory(): void {
    const now = new Date();
    this.loadExpenseHistory(now.getFullYear(), now.getMonth() + 1);
  }

  /**
   * 搜尋支出記錄
   */
  onSearch(): void {
    if (this.searchForm.valid) {
      const { year, month } = this.searchForm.value;
      this.loadExpenseHistory(year, month);
    }
  }

  /**
   * 載入支出記錄
   */
  private loadExpenseHistory(year: number, month: number): void {
    this.isLoading = true;
    this.error = null;

    this.expenseService.getExpenseHistory(year, month)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.expenseHistory = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('載入支出記錄失敗:', error);
          this.error = '載入支出記錄失敗，請稍後再試';
          this.expenseHistory = [];
          this.isLoading = false;
          this.showError('載入支出記錄失敗，請檢查網路連線或稍後再試。');
        }
      });
  }

  /**
   * 重新載入資料
   */
  refreshData(): void {
    if (this.searchForm.valid) {
      const { year, month } = this.searchForm.value;
      this.loadExpenseHistory(year, month);
    }
  }

  /**
   * 計算總支出
   */
  getTotalAmount(): number {
    return this.expenseHistory.reduce((total, expense) => total + expense.amount, 0);
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
   * 格式化日期顯示
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }

  /**
   * 取得選中月份的顯示名稱
   */
  getSelectedMonthName(): string {
    const monthValue = this.searchForm.get('month')?.value;
    const month = this.months.find(m => m.value === monthValue);
    return month ? month.name : '';
  }

  /**
   * 顯示錯誤訊息
   */
  private showError(message: string): void {
    this.snackBar.open(message, '關閉', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}