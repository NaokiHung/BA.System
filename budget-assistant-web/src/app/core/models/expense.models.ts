/**
 * 支出管理相關的資料模型
 * 對應後端的 DTOs
 */

export interface AddCashExpenseRequest {
  amount: number;
  description: string;
  category?: string;
}

export interface SetBudgetRequest {
  amount: number;
  year: number;
  month: number;
}

export interface ExpenseResponse {
  success: boolean;
  message: string;
  expenseId?: number;
  remainingBudget: number;
}

export interface MonthlyBudgetResponse {
  totalBudget: number;
  remainingCash: number;
  totalCashExpenses: number;
  totalSubscriptions: number;
  totalCreditCard: number;
  combinedCreditTotal: number;
  year: number;
  month: number;
  monthName: string;
}

export interface ExpenseHistory {
  id: number;
  amount: number;
  description: string;
  category: string;
  date: string;
}