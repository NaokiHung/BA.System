import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, AbstractControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/auth.models';

/**
 * 註冊組件 - Angular 19 獨立元件
 * 為什麼要進行即時的使用者名稱檢查？
 * 1. 提升使用者體驗，立即回饋
 * 2. 避免使用者填完整個表單後才發現帳號被占用
 * 3. 減少伺服器負擔（透過防抖動機制）
 */
@Component({
  selector: 'app-register',
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
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  registerForm!: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;
  isCheckingUsername = false;

  ngOnInit(): void {
    this.createForm();
    this.setupUsernameValidation();
  }

  /**
   * 建立註冊表單
   * 包含自訂驗證器：密碼確認、使用者名稱格式等
   */
  private createForm(): void {
    this.registerForm = this.fb.group({
      username: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern(/^[a-zA-Z0-9_]+$/) // 只允許英數字和底線
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        this.passwordValidator
      ]],
      confirmPassword: ['', [Validators.required]],
      email: ['', [Validators.email]],
      displayName: ['', [Validators.maxLength(100)]]
    }, {
      validators: this.passwordMatchValidator // 表單層級的驗證器
    });
  }

  /**
   * 設定使用者名稱即時驗證
   * 使用 RxJS 操作符來實現防抖動和去重
   */
  private setupUsernameValidation(): void {
    const usernameControl = this.registerForm.get('username');
    
    if (usernameControl) {
      usernameControl.valueChanges
        .pipe(
          debounceTime(500),        // 延遲 500ms 避免頻繁請求
          distinctUntilChanged(),   // 只有值真正改變時才處理
          switchMap(username => {   // 切換到新的請求，取消舊的請求
            if (username && username.length >= 3 && usernameControl.valid) {
              this.isCheckingUsername = true;
              return this.authService.checkUsername(username);
            }
            this.isCheckingUsername = false;
            return [];
          })
        )
        .subscribe({
          next: (result) => {
            this.isCheckingUsername = false;
            if (!result.available) {
              usernameControl.setErrors({ usernameTaken: true });
            }
          },
          error: () => {
            this.isCheckingUsername = false;
          }
        });
    }
  }

  /**
   * 自訂密碼驗證器
   * 檢查密碼強度（至少包含英文和數字）
   */
  private passwordValidator(control: AbstractControl): { [key: string]: any } | null {
    const value = control.value;
    
    if (!value) return null;

    const hasNumber = /[0-9]/.test(value);
    const hasLetter = /[a-zA-Z]/.test(value);
    
    if (!hasNumber || !hasLetter) {
      return { passwordWeak: true };
    }
    
    return null;
  }

  /**
   * 密碼確認驗證器
   * 檢查兩次輸入的密碼是否一致
   */
  private passwordMatchValidator(form: AbstractControl): { [key: string]: any } | null {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (!password || !confirmPassword) return null;
    
    if (password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    
    return null;
  }

  /**
   * 提交註冊表單
   */
  onSubmit(): void {
    if (this.registerForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const registerRequest: RegisterRequest = this.registerForm.value;
      
      this.authService.register(registerRequest).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('註冊成功！請登入您的帳號。', '關閉', { 
              duration: 5000,
              panelClass: ['success-snackbar']
            });
            this.router.navigate(['/auth/login']);
          } else {
            this.showError(response.message);
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('註冊錯誤:', error);
          this.showError('註冊失敗，請檢查網路連線或稍後再試。');
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  /**
   * 導航到登入頁面
   */
  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  /**
   * 取得表單控制項的錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    
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
    
    if (field?.hasError('email')) {
      return '請輸入有效的電子信箱格式';
    }
    
    if (field?.hasError('pattern')) {
      return '帳號只能包含英文、數字和底線';
    }
    
    if (field?.hasError('passwordWeak')) {
      return '密碼必須包含至少一個英文字母和一個數字';
    }
    
    if (field?.hasError('usernameTaken')) {
      return '此帳號已被使用';
    }
    
    return '';
  }

  /**
   * 取得表單層級的錯誤訊息（如密碼確認）
   */
  getFormErrorMessage(): string {
    if (this.registerForm.hasError('passwordMismatch')) {
      return '密碼與確認密碼不符';
    }
    
    return '';
  }

  /**
   * 取得欄位顯示名稱
   */
  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      username: '帳號',
      password: '密碼',
      confirmPassword: '確認密碼',
      email: '電子信箱',
      displayName: '顯示名稱'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 標記所有表單控制項為已觸碰
   */
  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
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