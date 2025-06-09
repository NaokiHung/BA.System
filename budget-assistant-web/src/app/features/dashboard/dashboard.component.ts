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
import { MonthlyBudgetResponse } from '../../core/models/expense.models';
import { User } from '../../core/models/auth.models'; // 修正：從正確的位置匯入 User

/**
 * 儀表板組件 - 修正匯入問題
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

  budgetUtilizationPercentage = 0;
  remainingDays = 0;
  currentMonth = '';

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadDashboardData();
    this.calculateRemainingDays();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private getCurrentUser(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });
  }

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

  private calculateBudgetStatistics(): void {
    if (this.budgetData) {
      if (this.budgetData.totalBudget > 0) {
        const usedAmount = this.budgetData.totalBudget - this.budgetData.remainingCash;
        this.budgetUtilizationPercentage = Math.round((usedAmount / this.budgetData.totalBudget) * 100);
      }

      this.currentMonth = this.budgetData.monthName;
    }
  }

  private calculateRemainingDays(): void {
    const now = new Date();
    const lastDayOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    this.remainingDays = Math.max(0, lastDayOfMonth.getDate() - now.getDate());
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: 'TWD',
      minimumFractionDigits: 0
    }).format(amount);
  }

  getBudgetStatusColor(): string {
    if (this.budgetUtilizationPercentage <= 60) return 'primary';
    if (this.budgetUtilizationPercentage <= 80) return 'accent';
    return 'warn';
  }

  navigateToExpense(): void {
    this.router.navigate(['/expense/add']);
  }

  navigateToBudget(): void {
    this.router.navigate(['/expense/budget']);
  }

  navigateToHistory(): void {
    this.router.navigate(['/expense/history']);
  }
}