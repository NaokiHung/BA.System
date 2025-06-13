/**
 * 檔案路徑: budget-assistant-web/src/app/features/auth/login/login.component.ts
 * 完整的登入組件
 */

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  loginForm!: FormGroup;
  isLoading = false;
  hidePassword = true;

  constructor() {
    console.log('🔑 LoginComponent 初始化');
  }

  ngOnInit(): void {
    this.createForm();
    console.log('🔑 LoginComponent ngOnInit 完成');
  }

  /**
   * 建立登入表單
   */
  private createForm(): void {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.maxLength(50)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
    
    console.log('📝 登入表單建立完成');
  }

  /**
   * 提交登入表單
   */
  onSubmit(): void {
    console.log('📤 提交登入表單');
    
    if (this.loginForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const loginRequest: LoginRequest = this.loginForm.value;
      console.log('📨 登入請求:', { username: loginRequest.username });
      
      this.authService.login(loginRequest).subscribe({
        next: (response) => {
          console.log('✅ 登入回應:', response);
          if (response.success) {
            this.snackBar.open('登入成功！', '關閉', { duration: 3000 });
            this.router.navigate(['/dashboard']);
          } else {
            this.showError(response.message);
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ 登入錯誤:', error);
          this.showError('登入失敗，請檢查網路連線或稍後再試。');
          this.isLoading = false;
        }
      });
    } else {
      console.log('❌ 表單無效，標記所有欄位');
      this.markFormGroupTouched();
    }
  }

  /**
   * 導航到註冊頁面
   */
  goToRegister(): void {
    console.log('📝 導航到註冊頁面');
    this.router.navigate(['/auth/register']);
  }

  /**
   * 取得表單控制項的錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)}不能為空`;
    }
    
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `${this.getFieldDisplayName(fieldName)}至少需要${minLength}個字元`;
    }
    
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `${this.getFieldDisplayName(fieldName)}不能超過${maxLength}個字元`;
    }
    
    return '';
  }

  /**
   * 取得欄位顯示名稱
   */
  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      username: '帳號',
      password: '密碼'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 標記所有表單控制項為已觸碰
   */
  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
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