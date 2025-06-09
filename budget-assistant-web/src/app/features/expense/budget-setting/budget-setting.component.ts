import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExpenseService } from '../../../core/services/expense.service';
import { SetBudgetRequest } from '../../../core/models/expense.models';

/**
 * 預算設定組件 - Angular 19 獨立元件
 * 為什麼需要獨立的預算設定？
 * 1. 預算是財務管理的基礎
 * 2. 提供靈活的月份選擇
 * 3. 支援預算的新增和修改
 */
@Component({
  selector: 'app-budget-setting',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './budget-setting.component.html',
  styleUrls: ['./budget-setting.component.scss']
})
export class BudgetSettingComponent implements OnInit {
  private fb = inject(FormBuilder);
  private expenseService = inject(ExpenseService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  budgetForm!: FormGroup;
  isLoading = false;
  
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
  }

  /**
   * 產生年份選項（當前年份前後各2年）
   */
  private generateYearOptions(): void {
    const currentYear = new Date().getFullYear();
    for (let year = currentYear - 2; year <= currentYear + 2; year++) {
      this.years.push(year);
    }
  }

  /**
   * 建立預算設定表單
   */
  private createForm(): void {
    const now = new Date();
    
    this.budgetForm = this.fb.group({
      amount: ['', [
        Validators.required,
        Validators.min(1),
        Validators.max(9999999.99)
      ]],
      year: [now.getFullYear(), [
        Validators.required,
        Validators.min(2020),
        Validators.max(2050)
      ]],
      month: [now.getMonth() + 1, [
        Validators.required,
        Validators.min(1),
        Validators.max(12)
      ]]
    });
  }

  /**
   * 提交預算設定表單
   */
  onSubmit(): void {
    if (this.budgetForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const request: SetBudgetRequest = this.budgetForm.value;
      
      this.expenseService.setMonthlyBudget(request).subscribe({
        next: (response) => {
          if (response.success) {
            const monthName = this.months.find(m => m.value === request.month)?.name;
            this.snackBar.open(
              `${request.year}年${monthName}預算設定成功！`, 
              '關閉', 
              { duration: 5000, panelClass: ['success-snackbar'] }
            );
            this.router.navigate(['/dashboard']);
          } else {
            this.showError(response.message);
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('設定預算錯誤:', error);
          this.showError('設定預算失敗，請檢查網路連線或稍後再試。');
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  /**
   * 取消操作
   */
  onCancel(): void {
    this.router.navigate(['/dashboard']);
  }

  /**
   * 設定當前月份預算
   */
  setCurrentMonth(): void {
    const now = new Date();
    this.budgetForm.patchValue({
      year: now.getFullYear(),
      month: now.getMonth() + 1
    });
  }

  /**
   * 設定下個月預算
   */
  setNextMonth(): void {
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    
    this.budgetForm.patchValue({
      year: nextMonth.getFullYear(),
      month: nextMonth.getMonth() + 1
    });
  }

  /**
   * 取得表單控制項的錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.budgetForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)}不能為空`;
    }
    
    if (field?.hasError('min')) {
      const minValue = field.errors?.['min'].min;
      return `${this.getFieldDisplayName(fieldName)}不能小於 ${minValue}`;
    }
    
    if (field?.hasError('max')) {
      const maxValue = field.errors?.['max'].max;
      return `${this.getFieldDisplayName(fieldName)}不能大於 ${maxValue}`;
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
   * 取得欄位顯示名稱
   */
  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      amount: '預算金額',
      year: '年份',
      month: '月份'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 標記所有表單控制項為已觸碰
   */
  private markFormGroupTouched(): void {
    Object.keys(this.budgetForm.controls).forEach(key => {
      const control = this.budgetForm.get(key);
      control?.markAsTouched();
    });
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