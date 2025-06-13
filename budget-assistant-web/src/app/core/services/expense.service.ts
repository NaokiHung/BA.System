import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddCashExpenseRequest,
  AddCreditCardExpenseRequest,
  UpdateExpenseRequest,
  SetBudgetRequest,
  ExpenseResponse,
  MonthlyBudgetResponse,
  ExpenseHistory,
  ExpenseDetailResponse
} from '../models/expense.models';

/**
 * 增強版支出管理服務
 * 新增信用卡支出、編輯、刪除等功能
 * 與後端 ExpenseController 的 API 對應
 */
@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private apiUrl = `${environment.apiUrl}/expense`;

  constructor(private http: HttpClient) {}

  // === 預算管理 ===

  /**
   * 取得當月預算資訊
   * 對應後端 ExpenseController.GetCurrentMonthBudget
   */
  getCurrentMonthBudget(): Observable<MonthlyBudgetResponse> {
    return this.http.get<MonthlyBudgetResponse>(`${this.apiUrl}/budget/current`);
  }

  /**
   * 設定月預算
   * 對應後端 ExpenseController.SetMonthlyBudget
   */
  setMonthlyBudget(request: SetBudgetRequest): Observable<ExpenseResponse> {
    return this.http.post<ExpenseResponse>(`${this.apiUrl}/budget`, request);
  }

  // === 支出記錄管理 ===

  /**
   * 新增現金支出
   * 對應後端 ExpenseController.AddCashExpense
   * 為什麼分開現金和信用卡？
   * 現金支出會立即影響預算餘額，信用卡支出不會
   */
  addCashExpense(request: AddCashExpenseRequest): Observable<ExpenseResponse> {
    return this.http.post<ExpenseResponse>(`${this.apiUrl}/cash`, request);
  }

  /**
   * 新增信用卡支出
   * 對應後端 ExpenseController.AddCreditCardExpense
   * 為什麼需要信用卡支出？
   * 1. 信用卡消費不會立即影響現金流
   * 2. 需要分別統計現金和信用卡支出
   * 3. 便於後續信用卡帳單管理功能
   */
  addCreditCardExpense(request: AddCreditCardExpenseRequest): Observable<ExpenseResponse> {
    return this.http.post<ExpenseResponse>(`${this.apiUrl}/credit-card`, request);
  }

  /**
   * 更新支出記錄
   * 對應後端 ExpenseController.UpdateExpense
   * 為什麼需要更新功能？
   * 使用者可能輸入錯誤或需要調整記錄內容
   */
  updateExpense(expenseId: number, request: UpdateExpenseRequest): Observable<ExpenseResponse> {
    return this.http.put<ExpenseResponse>(`${this.apiUrl}/${expenseId}`, request);
  }

  /**
   * 刪除支出記錄
   * 對應後端 ExpenseController.DeleteExpense
   * 為什麼需要刪除功能？
   * 1. 使用者可能重複記錄
   * 2. 記錄錯誤需要移除
   * 3. 提供完整的 CRUD 操作
   */
  deleteExpense(expenseId: number): Observable<ExpenseResponse> {
    return this.http.delete<ExpenseResponse>(`${this.apiUrl}/${expenseId}`);
  }

  /**
   * 取得支出記錄詳情
   * 對應後端 ExpenseController.GetExpenseDetail
   * 為什麼需要詳情 API？
   * 編輯功能需要載入完整的記錄資料
   */
  getExpenseDetail(expenseId: number): Observable<ExpenseDetailResponse> {
    return this.http.get<ExpenseDetailResponse>(`${this.apiUrl}/${expenseId}`);
  }

  /**
   * 取得支出歷史記錄
   * 對應後端 ExpenseController.GetExpenseHistory
   */
  getExpenseHistory(year: number, month: number): Observable<ExpenseHistory[]> {
    return this.http.get<ExpenseHistory[]>(`${this.apiUrl}/history/${year}/${month}`);
  }

  // === 統計和分析 ===

  /**
   * 取得支出統計資料
   * 為使用者提供消費分析
   * 未來可擴展為更詳細的統計功能
   */
  getExpenseStatistics(year: number, month: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/statistics/${year}/${month}`);
  }

  /**
   * 取得類別支出統計
   * 幫助使用者了解各類別的消費比例
   */
  getCategoryStatistics(year: number, month: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/category-stats/${year}/${month}`);
  }

  // === 輔助方法 ===

  /**
   * 驗證支出記錄是否可編輯
   * 前端驗證，減少不必要的 API 呼叫
   */
  canEditExpense(expenseDate: string): boolean {
    const expense = new Date(expenseDate);
    const now = new Date();
    const oneMonthAgo = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
    
    // 只允許編輯一個月內的記錄
    return expense >= oneMonthAgo;
  }

  /**
   * 驗證支出記錄是否可刪除
   * 前端驗證，減少不必要的 API 呼叫
   */
  canDeleteExpense(expenseDate: string): boolean {
    const expense = new Date(expenseDate);
    const now = new Date();
    const currentMonthStart = new Date(now.getFullYear(), now.getMonth(), 1);
    
    // 只允許刪除當月的記錄
    return expense >= currentMonthStart;
  }

  /**
   * 格式化金額
   * 統一的金額格式化方法
   */
  formatAmount(amount: number): string {
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: 'TWD',
      minimumFractionDigits: 0
    }).format(amount);
  }

  /**
   * 格式化日期
   * 統一的日期格式化方法
   */
  formatDate(date: string | Date): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(dateObj);
  }

  /**
   * 計算預算使用率
   * 幫助使用者了解預算使用情況
   */
  calculateBudgetUtilization(totalBudget: number, remainingBudget: number): number {
    if (totalBudget <= 0) return 0;
    const used = totalBudget - remainingBudget;
    return Math.round((used / totalBudget) * 100);
  }

  /**
   * 取得預算狀態顏色
   * 根據使用率返回對應的顏色主題
   */
  getBudgetStatusColor(utilizationPercentage: number): 'primary' | 'accent' | 'warn' {
    if (utilizationPercentage < 70) return 'primary';
    if (utilizationPercentage < 90) return 'accent';
    return 'warn';
  }

  /**
   * 驗證金額格式
   * 統一的金額驗證邏輯
   */
  validateAmount(amount: number): { valid: boolean; message?: string } {
    if (amount <= 0) {
      return { valid: false, message: '金額必須大於 0' };
    }
    
    if (amount > 9999999.99) {
      return { valid: false, message: '金額不能超過 9,999,999.99' };
    }
    
    // 檢查小數位數
    if (Number(amount.toFixed(2)) !== amount) {
      return { valid: false, message: '金額最多只能有兩位小數' };
    }
    
    return { valid: true };
  }

  /**
   * 驗證描述格式
   * 統一的描述驗證邏輯
   */
  validateDescription(description: string): { valid: boolean; message?: string } {
    if (!description || description.trim().length === 0) {
      return { valid: false, message: '描述不能為空' };
    }
    
    if (description.length > 200) {
      return { valid: false, message: '描述長度不能超過 200 個字元' };
    }
    
    return { valid: true };
  }
}