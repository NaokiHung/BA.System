import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddCashExpenseRequest,
  SetBudgetRequest,
  ExpenseResponse,
  MonthlyBudgetResponse,
  ExpenseHistory
} from '../models/expense.models';

/**
 * 支出管理服務
 * 與後端 ExpenseController 的 API 對應
 */
@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private apiUrl = `${environment.apiUrl}/expense`;

  constructor(private http: HttpClient) {}

  /**
   * 取得當月預算資訊
   * 對應後端 ExpenseController.GetCurrentMonthBudget
   */
  getCurrentMonthBudget(): Observable<MonthlyBudgetResponse> {
    return this.http.get<MonthlyBudgetResponse>(`${this.apiUrl}/budget/current`);
  }

  /**
   * 新增現金支出
   * 對應後端 ExpenseController.AddCashExpense
   */
  addCashExpense(request: AddCashExpenseRequest): Observable<ExpenseResponse> {
    return this.http.post<ExpenseResponse>(`${this.apiUrl}/cash`, request);
  }

  /**
   * 設定月預算
   * 對應後端 ExpenseController.SetMonthlyBudget
   */
  setMonthlyBudget(request: SetBudgetRequest): Observable<ExpenseResponse> {
    return this.http.post<ExpenseResponse>(`${this.apiUrl}/budget`, request);
  }

  /**
   * 取得支出歷史記錄
   * 對應後端 ExpenseController.GetExpenseHistory
   */
  getExpenseHistory(year: number, month: number): Observable<ExpenseHistory[]> {
    return this.http.get<ExpenseHistory[]>(`${this.apiUrl}/history/${year}/${month}`);
  }
}