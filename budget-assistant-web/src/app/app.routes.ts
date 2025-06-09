import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { authGuard, noAuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
      {
        path: 'auth',
        canActivate: [noAuthGuard],
        loadChildren: () => import('./features/auth/auth.routes').then(r => r.authRoutes)
      },
      {
        path: 'dashboard',
        canActivate: [authGuard],
        loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent)
      },
      {
        path: 'expense',
        canActivate: [authGuard],
        loadChildren: () => import('./features/expense/expense.routes').then(r => r.expenseRoutes)
      }
    ]
  },
  { path: '**', redirectTo: '/dashboard' }
];