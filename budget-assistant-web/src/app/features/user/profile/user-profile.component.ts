import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import { 
  User, 
  UpdateUserProfileRequest, 
  ChangePasswordRequest,
  UserProfileResponse 
} from '../../../core/models/expense.models';

/**
 * 使用者資料管理組件
 * 檔案路徑：budget-assistant-web/src/app/features/user/profile/user-profile.component.ts
 * 
 * 為什麼要獨立使用者管理？
 * 1. 提供完整的帳戶管理功能
 * 2. 安全的密碼變更機制
 * 3. 使用者資料的維護和更新
 * 4. 符合功能模組化的設計原則
 */
@Component({
  selector: 'app-user-profile',
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
    MatSnackBarModule,
    MatTabsModule,
    MatDividerModule
  ],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);
  private destroy$ = new Subject<void>();

  currentUser: User | null = null;
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  // 載入狀態
  isUpdatingProfile = false;
  isChangingPassword = false;

  // 密碼可見性控制
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  // 帳戶統計資料
  registrationDate: string | null = null;
  lastLoginDate: string | null = null;
  totalExpenseRecords: number = 0;
  totalBudgets: number = 0;

  ngOnInit(): void {
    this.getCurrentUser();
    this.createForms();
    this.loadAccountStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 取得當前使用者資訊
   * 為什麼要監聽 currentUser$？
   * 使用者資料可能在其他地方被更新，需要保持同步
   */
  private getCurrentUser(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user) {
          this.loadUserProfile();
        }
      });
  }

  /**
   * 載入使用者完整資料
   * 為什麼要重新載入？
   * AuthService 中的使用者資料可能不完整，需要從後端取得完整資料
   */
  private loadUserProfile(): void {
    if (this.currentUser) {
      // 先使用當前認證用戶資料進行初始化
      console.log('使用當前認證用戶資料:', this.currentUser);
      this.updateProfileForm();
      
      // 嘗試載入完整的用戶資料
      this.userService.getUserProfile()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (profile: User) => {
            console.log('成功載入完整用戶資料:', profile);
            this.currentUser = profile;
            this.updateProfileForm();
          },
          error: (error: any) => {
            console.warn('無法載入完整用戶資料，繼續使用認證資料:', error);
            // 如果API失敗，繼續使用認證中的用戶資料，不顯示錯誤
          }
        });
    }
  }

  /**
   * 建立表單
   * 為什麼要分開建立兩個表單？
   * 1. 個人資料和密碼變更是不同的業務邏輯
   * 2. 密碼變更需要額外的驗證邏輯
   * 3. 分開管理更易於維護和測試
   */
  private createForms(): void {
    // 個人資料表單
    this.profileForm = this.fb.group({
      displayName: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(50)
      ]],
      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(100)
      ]]
    });

    // 密碼變更表單
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(50),
        this.passwordValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  /**
   * 更新個人資料表單資料
   * 為什麼使用 patchValue？
   * patchValue 只更新指定的欄位，不會影響其他欄位的狀態
   */
  private updateProfileForm(): void {
    if (this.currentUser) {
      console.log('更新個人資料表單:', this.currentUser);
      this.profileForm.patchValue({
        displayName: this.currentUser.displayName || '',
        email: this.currentUser.email || ''
      });
      // 標記表單為 pristine，避免顯示驗證錯誤
      this.profileForm.markAsPristine();
    }
  }

  /**
   * 載入帳戶統計資料
   * 為什麼要載入統計資料？
   * 提供使用者帳戶的整體使用情況，增加使用者對系統的了解
   */
  private loadAccountStatistics(): void {
    this.userService.getAccountStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats: any) => {
          this.registrationDate = this.formatDate(stats.registrationDate);
          this.lastLoginDate = this.formatDate(stats.lastLoginDate);
          this.totalExpenseRecords = stats.totalExpenseRecords;
          this.totalBudgets = stats.totalBudgets;
        },
        error: (error: any) => {
          console.error('載入帳戶統計失敗:', error);
          // 統計資料載入失敗不影響主要功能，只記錄錯誤
        }
      });
  }

  /**
   * 更新個人資料
   * 為什麼要檢查 isUpdatingProfile？
   * 防止使用者重複點擊提交按鈕，避免重複的 API 呼叫
   */
  updateProfile(): void {
    if (this.profileForm.valid && !this.isUpdatingProfile) {
      this.isUpdatingProfile = true;

      const request: UpdateUserProfileRequest = {
        displayName: this.profileForm.value.displayName.trim(),
        email: this.profileForm.value.email.trim()
      };

      this.userService.updateProfile(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response: UserProfileResponse) => {
            if (response.success) {
              this.showSuccess('個人資料更新成功！');
              if (response.user) {
                this.currentUser = response.user;
                // 更新 AuthService 中的使用者資料，保持全域狀態同步
                this.authService.updateCurrentUser(response.user);
              }
            } else {
              this.showError(response.message);
            }
            this.isUpdatingProfile = false;
          },
          error: (error: any) => {
            console.error('更新個人資料失敗:', error);
            this.showError('更新失敗，請檢查網路連線或稍後再試');
            this.isUpdatingProfile = false;
          }
        });
    }
  }

  /**
   * 變更密碼
   * 為什麼密碼變更需要確認當前密碼？
   * 安全考量，確保是帳戶擁有者本人操作，防止未授權的密碼變更
   */
  changePassword(): void {
    if (this.passwordForm.valid && !this.isChangingPassword) {
      this.isChangingPassword = true;

      const request: ChangePasswordRequest = {
        currentPassword: this.passwordForm.value.currentPassword,
        newPassword: this.passwordForm.value.newPassword,
        confirmPassword: this.passwordForm.value.confirmPassword
      };

      this.userService.changePassword(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response: UserProfileResponse) => {
            if (response.success) {
              this.showSuccess('密碼變更成功！');
              this.resetPasswordForm();
            } else {
              this.showError(response.message);
            }
            this.isChangingPassword = false;
          },
          error: (error: any) => {
            console.error('變更密碼失敗:', error);
            this.showError('密碼變更失敗，請檢查目前密碼是否正確');
            this.isChangingPassword = false;
          }
        });
    }
  }

  /**
   * 重設個人資料表單
   * 恢復到原始資料
   */
  resetProfileForm(): void {
    this.updateProfileForm();
  }

  /**
   * 重設密碼表單
   * 清空所有密碼欄位
   */
  resetPasswordForm(): void {
    this.passwordForm.reset();
  }

  /**
   * 取得個人資料表單錯誤訊息
   */
  getProfileErrorMessage(fieldName: string): string {
    const field = this.profileForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)}不能為空`;
    }
    
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength']?.requiredLength;
      return `長度至少需要 ${minLength} 個字元`;
    }
    
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength']?.requiredLength;
      return `長度不能超過 ${maxLength} 個字元`;
    }
    
    if (field?.hasError('email')) {
      return '請輸入有效的電子郵件地址';
    }
    
    return '';
  }

  /**
   * 取得密碼表單錯誤訊息
   */
  getPasswordErrorMessage(fieldName: string): string {
    const field = this.passwordForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getPasswordFieldDisplayName(fieldName)}不能為空`;
    }
    
    if (field?.hasError('minlength')) {
      return '密碼長度至少需要 6 個字元';
    }
    
    if (field?.hasError('maxlength')) {
      return '密碼長度不能超過 50 個字元';
    }
    
    if (field?.hasError('invalidPassword')) {
      return '密碼必須包含至少一個英文字母和一個數字';
    }
    
    if (fieldName === 'confirmPassword' && this.passwordForm.hasError('passwordMismatch')) {
      return '確認密碼與新密碼不符';
    }
    
    return '';
  }

  /**
   * 密碼驗證器
   * 檢查密碼是否包含字母和數字
   */
  private passwordValidator(control: any) {
    const value = control.value;
    if (!value) return null;
    
    const hasLetter = /[a-zA-Z]/.test(value);
    const hasNumber = /\d/.test(value);
    
    if (hasLetter && hasNumber) {
      return null;
    }
    
    return { invalidPassword: true };
  }

  /**
   * 密碼確認驗證器
   * 確保新密碼和確認密碼相符
   */
  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    
    return null;
  }

  /**
   * 取得欄位顯示名稱
   */
  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'displayName': '顯示名稱',
      'email': '電子郵件'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 取得密碼欄位顯示名稱
   */
  private getPasswordFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'currentPassword': '目前密碼',
      'newPassword': '新密碼',
      'confirmPassword': '確認密碼'
    };
    return displayNames[fieldName] || fieldName;
  }

  /**
   * 格式化日期
   */
  private formatDate(dateString: string): string {
    if (!dateString) return '';
    
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