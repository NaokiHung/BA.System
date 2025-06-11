import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { ExpenseService } from '../../../core/services/expense.service';
import { 
  ExpenseHistory, 
  ExpenseTableItem, 
  ExpenseType,
  UpdateExpenseRequest 
} from '../../../core/models/expense.models';

/**
 * 增強版支出記錄組件 - 支援內聯編輯和刪除
 * 為什麼選擇內聯編輯？
 * 1. 更直觀的使用者體驗，不需要跳轉頁面
 * 2. 可以同時檢視其他記錄，便於比較
 * 3. 減少操作步驟，提升效率
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
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatMenuModule,
    MatChipsModule
  ],
  templateUrl: './expense-history.component.html',
  styleUrls: ['./expense-history.component.scss']
})
export class ExpenseHistoryComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private expenseService = inject(ExpenseService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroy$ = new Subject<void>();

  searchForm!: FormGroup;
  expenseHistory: ExpenseTableItem[] = [];
  isLoading = false;
  error: string | null = null;

  // 編輯相關狀態
  editingExpenseId: number | null = null;
  editForm!: FormGroup;
  isSaving = false;

  // 表格顯示的欄位
  displayedColumns: string[] = ['date', 'description', 'category', 'amount', 'type', 'actions'];

  // 支出類別選項
  expenseCategories = [
    '餐飲', '交通', '購物', '娛樂', '醫療', '教育', '家用', '其他'
  ];

  // 年份和月份選項
  years: number[] = [];
  months = [
    { value: 1, name: '一月' }, { value: 2, name: '二月' }, { value: 3, name: '三月' },
    { value: 4, name: '四月' }, { value: 5, name: '五月' }, { value: 6, name: '六月' },
    { value: 7, name: '七月' }, { value: 8, name: '八月' }, { value: 9, name: '九月' },
    { value: 10, name: '十月' }, { value: 11, name: '十一月' }, { value: 12, name: '十二月' }
  ];

  ngOnInit(): void {
    this.generateYearOptions();
    this.createForms();
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
   * 建立表單
   */
  private createForms(): void {
    const now = new Date();
    
    // 搜尋表單
    this.searchForm = this.fb.group({
      year: [now.getFullYear(), [Validators.required]],
      month: [now.getMonth() + 1, [Validators.required]]
    });

    // 編輯表單
    this.editForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(0.01), Validators.max(999999.99)]],
      description: ['', [Validators.required, Validators.maxLength(200)]],
      category: ['', [Validators.maxLength(50)]]
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
    this.cancelEdit(); // 取消任何正在進行的編輯

    this.expenseService.getExpenseHistory(year, month)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.expenseHistory = this.transformToTableItems(data);
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
   * 轉換為表格顯示項目
   * 為什麼要轉換？
   * 加入前端專用的格式化欄位和顯示邏輯
   */
  private transformToTableItems(data: ExpenseHistory[]): ExpenseTableItem[] {
    return data.map(item => ({
      ...item,
      formattedAmount: this.formatCurrency(item.amount),
      formattedDate: this.formatDate(item.date),
      typeDisplayName: this.getExpenseTypeDisplayName(item.expenseType),
      isEditing: false
    }));
  }

  /**
   * 開始編輯支出記錄
   */
  startEdit(expense: ExpenseTableItem): void {
    if (!expense.canEdit) {
      this.showError('此記錄無法編輯');
      return;
    }

    // 取消其他正在編輯的項目
    this.cancelEdit();

    // 設定編輯狀態
    this.editingExpenseId = expense.id;
    expense.isEditing = true;

    // 載入資料到編輯表單
    this.editForm.patchValue({
      amount: expense.amount,
      description: expense.description,
      category: expense.category
    });
  }

  /**
   * 取消編輯
   */
  cancelEdit(): void {
    if (this.editingExpenseId !== null) {
      const editingItem = this.expenseHistory.find(item => item.id === this.editingExpenseId);
      if (editingItem) {
        editingItem.isEditing = false;
      }
      this.editingExpenseId = null;
      this.editForm.reset();
    }
  }

  /**
   * 儲存編輯
   */
  saveEdit(expense: ExpenseTableItem): void {
    if (this.editForm.valid && !this.isSaving) {
      this.isSaving = true;

      const request: UpdateExpenseRequest = {
        amount: this.editForm.value.amount,
        description: this.editForm.value.description.trim(),
        category: this.editForm.value.category || '其他',
        expenseType: expense.expenseType
      };

      this.expenseService.updateExpense(expense.id, request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.showSuccess('支出記錄更新成功！');
              this.cancelEdit();
              // 重新載入當前頁面的資料
              const { year, month } = this.searchForm.value;
              this.loadExpenseHistory(year, month);
            } else {
              this.showError(response.message);
            }
            this.isSaving = false;
          },
          error: (error) => {
            console.error('更新支出記錄失敗:', error);
            this.showError('更新失敗，請稍後再試');
            this.isSaving = false;
          }
        });
    }
  }

  /**
   * 刪除支出記錄
   * 為什麼需要確認對話框？
   * 刪除是不可逆操作，需要使用者明確確認
   */
  deleteExpense(expense: ExpenseTableItem): void {
    if (!expense.canDelete) {
      this.showError('此記錄無法刪除');
      return;
    }

    // 顯示確認對話框
    const confirmed = confirm(`確定要刪除這筆支出記錄嗎？\n\n描述：${expense.description}\n金額：${expense.formattedAmount}`);
    
    if (confirmed) {
      this.expenseService.deleteExpense(expense.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.showSuccess('支出記錄已刪除');
              // 重新載入當前頁面的資料
              const { year, month } = this.searchForm.value;
              this.loadExpenseHistory(year, month);
            } else {
              this.showError(response.message);
            }
          },
          error: (error) => {
            console.error('刪除支出記錄失敗:', error);
            this.showError('刪除失敗，請稍後再試');
          }
        });
    }
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
   * 取得支出類型顯示名稱
   */
  getExpenseTypeDisplayName(type: ExpenseType): string {
    switch (type) {
      case ExpenseType.Cash:
        return '現金';
      case ExpenseType.CreditCard:
        return '信用卡';
      default:
        return '未知';
    }
  }

  /**
   * 取得支出類型顯示顏色
   */
  getExpenseTypeColor(type: ExpenseType): string {
    switch (type) {
      case ExpenseType.Cash:
        return 'primary';
      case ExpenseType.CreditCard:
        return 'accent';
      default:
        return '';
    }
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
   * 取得編輯表單錯誤訊息
   */
  getEditErrorMessage(fieldName: string): string {
    const field = this.editForm.get(fieldName);
    
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
      const maxLength = field.errors?.['maxlength']?.requiredLength;
      return `長度不能超過 ${maxLength} 個字元`;
    }
    
    return '';
  }

  /**
   * 取得欄位顯示名稱
   */
  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'amount': '金額',
      'description': '描述',
      'category': '類別'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 顯示成功訊息
   */
  private showSuccess(message: string): void {
    this.snackBar.open(message, '關閉', {
      duration: 3000,
      panelClass: ['success-snackbar']
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