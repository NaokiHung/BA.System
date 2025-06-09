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
import { AddCashExpenseRequest } from '../../../core/models/expense.models';

/**
 * 新增支出組件 - Angular 19 獨立元件
 * 為什麼要獨立成一個組件？
 * 1. 單一職責原則，專門處理支出新增
 * 2. 可重複使用（例如在彈出視窗中使用）
 * 3. 易於測試和維護
 */
@Component({
  selector: 'app-add-expense',
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
  templateUrl: './add-expense.component.html',
  styleUrls: ['./add-expense.component.scss']
})
export class AddExpenseComponent implements OnInit {
  private fb = inject(FormBuilder);
  private expenseService = inject(ExpenseService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  expenseForm!: FormGroup;
  isLoading = false;
  
  // 預定義的支出類別
  expenseCategories = [
    '餐飲',
    '交通',
    '購物',
    '娛樂',
    '醫療',
    '教育',
    '家用',
    '其他'
  ];

  ngOnInit(): void {
    this.createForm();
  }

  /**
   * 建立支出表單
   */
  private createForm(): void {
    this.expenseForm = this.fb.group({
      amount: ['', [
        Validators.required,
        Validators.min(0.01),
        Validators.max(999999.99)
      ]],
      description: ['', [
        Validators.required,
        Validators.maxLength(200)
      ]],
      category: ['', [
        Validators.maxLength(50)
      ]]
    });
  }

  /**
   * 提交支出表單
   */
  onSubmit(): void {
    if (this.expenseForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const request: AddCashExpenseRequest = {
        amount: this.expenseForm.value.amount,
        description: this.expenseForm.value.description.trim(),
        category: this.expenseForm.value.category || '其他'
      };
      
      this.expenseService.addCashExpense(request).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open(
              `支出新增成功！剩餘預算：${this.formatCurrency(response.remainingBudget)}`, 
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
          console.error('新增支出錯誤:', error);
          this.showError('新增支出失敗，請檢查網路連線或稍後再試。');
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  /**
   * 取消操作，返回儀表板
   */
  onCancel(): void {
    this.router.navigate(['/dashboard']);
  }

  /**
   * 設定快速金額
   */
  setQuickAmount(amount: number): void {
    this.expenseForm.patchValue({ amount: amount });
  }

  /**
   * 取得表單控制項的錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.expenseForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)}不能為空`;
    }
    
    if (field?.hasError('min')) {
      return '金額必須大於 0';
    }
    
    if (field?.hasError('max')) {
      return '金額不能超過 999,999.99';
    }
    
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `${this.getFieldDisplayName(fieldName)}不能超過${maxLength}個字元`;
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
      amount: '金額',
      description: '描述',
      category: '類別'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 標記所有表單控制項為已觸碰
   */
  private markFormGroupTouched(): void {
    Object.keys(this.expenseForm.controls).forEach(key => {
      const control = this.expenseForm.get(key);
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