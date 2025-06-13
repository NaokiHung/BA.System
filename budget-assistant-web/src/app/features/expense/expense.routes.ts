import { Routes } from '@angular/router';
import { AddExpenseComponent } from './add-expense/add-expense.component';
import { BudgetSettingComponent } from './budget-setting/budget-setting.component';
import { ExpenseHistoryComponent } from './expense-history/expense-history.component';

/**
 * 支出管理模組路由
 * 為什麼要細分這麼多路由？
 * 1. 每個功能都有獨立的頁面，使用者體驗更好
 * 2. 支援深層連結，使用者可以直接分享特定功能的 URL
 * 3. 便於權限控制和功能擴展
 */
export const expenseRoutes: Routes = [
  { path: '', redirectTo: 'history', pathMatch: 'full' },
  { path: 'add', component: AddExpenseComponent },
  { path: 'budget', component: BudgetSettingComponent },
  { path: 'history', component: ExpenseHistoryComponent }
];