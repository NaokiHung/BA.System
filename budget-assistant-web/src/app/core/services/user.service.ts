import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  User, 
  UpdateUserProfileRequest, 
  ChangePasswordRequest,
  UserProfileResponse 
} from '../models/expense.models';

/**
 * 使用者管理服務
 * 檔案路徑：budget-assistant-web/src/app/core/services/user.service.ts
 * 負責使用者資料的 CRUD 操作
 * 與後端 UserController 的 API 對應
 */
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/user`;

  constructor(private http: HttpClient) {}

  /**
   * 取得使用者完整資料
   * 對應後端 UserController.GetProfile
   */
  getUserProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/profile`);
  }

  /**
   * 更新使用者個人資料
   * 對應後端 UserController.UpdateProfile
   */
  updateProfile(request: UpdateUserProfileRequest): Observable<UserProfileResponse> {
    return this.http.put<UserProfileResponse>(`${this.apiUrl}/profile`, request);
  }

  /**
   * 變更使用者密碼
   * 對應後端 UserController.ChangePassword
   */
  changePassword(request: ChangePasswordRequest): Observable<UserProfileResponse> {
    return this.http.put<UserProfileResponse>(`${this.apiUrl}/change-password`, request);
  }

  /**
   * 取得帳戶統計資料
   * 對應後端 UserController.GetAccountStatistics
   */
  getAccountStatistics(): Observable<{
    registrationDate: string;
    lastLoginDate: string;
    totalExpenseRecords: number;
    totalBudgets: number;
    totalCashExpenses: number;
    totalCreditCardExpenses: number;
    averageMonthlyExpense: number;
  }> {
    return this.http.get<{
      registrationDate: string;
      lastLoginDate: string;
      totalExpenseRecords: number;
      totalBudgets: number;
      totalCashExpenses: number;
      totalCreditCardExpenses: number;
      averageMonthlyExpense: number;
    }>(`${this.apiUrl}/statistics`);
  }

  /**
   * 驗證使用者名稱是否可用
   * 對應後端 UserController.CheckUsernameAvailability
   * 用於註冊或使用者名稱變更時的驗證
   */
  checkUsernameAvailability(username: string): Observable<{ available: boolean }> {
    return this.http.get<{ available: boolean }>(`${this.apiUrl}/check-username/${username}`);
  }

  /**
   * 驗證電子郵件是否可用
   * 對應後端 UserController.CheckEmailAvailability
   * 用於註冊或電子郵件變更時的驗證
   */
  checkEmailAvailability(email: string): Observable<{ available: boolean }> {
    return this.http.get<{ available: boolean }>(`${this.apiUrl}/check-email/${email}`);
  }

  /**
   * 上傳使用者頭像
   * 對應後端 UserController.UploadAvatar
   * 未來可擴展的功能
   */
  uploadAvatar(file: File): Observable<UserProfileResponse> {
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<UserProfileResponse>(`${this.apiUrl}/avatar`, formData);
  }

  /**
   * 刪除使用者頭像
   * 對應後端 UserController.DeleteAvatar
   */
  deleteAvatar(): Observable<UserProfileResponse> {
    return this.http.delete<UserProfileResponse>(`${this.apiUrl}/avatar`);
  }

  /**
   * 取得使用者偏好設定
   * 對應後端 UserController.GetPreferences
   * 包含主題、語言、通知設定等
   */
  getUserPreferences(): Observable<any> {
    return this.http.get(`${this.apiUrl}/preferences`);
  }

  /**
   * 更新使用者偏好設定
   * 對應後端 UserController.UpdatePreferences
   */
  updateUserPreferences(preferences: any): Observable<UserProfileResponse> {
    return this.http.put<UserProfileResponse>(`${this.apiUrl}/preferences`, preferences);
  }

  /**
   * 停用帳戶
   * 對應後端 UserController.DeactivateAccount
   * 軟刪除：帳戶標記為非活躍，但保留資料
   */
  deactivateAccount(reason?: string): Observable<UserProfileResponse> {
    return this.http.post<UserProfileResponse>(`${this.apiUrl}/deactivate`, { reason });
  }

  /**
   * 匯出使用者資料
   * 對應後端 UserController.ExportData
   * 符合 GDPR 等資料保護法規要求
   */
  exportUserData(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, { responseType: 'blob' });
  }

  // === 輔助方法 ===

  /**
   * 驗證電子郵件格式
   * 前端驗證，減少不必要的 API 呼叫
   */
  validateEmail(email: string): { valid: boolean; message?: string } {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (!email) {
      return { valid: false, message: '電子郵件不能為空' };
    }
    
    if (!emailRegex.test(email)) {
      return { valid: false, message: '請輸入有效的電子郵件地址' };
    }
    
    if (email.length > 100) {
      return { valid: false, message: '電子郵件長度不能超過 100 個字元' };
    }
    
    return { valid: true };
  }

  /**
   * 驗證顯示名稱格式
   * 前端驗證，減少不必要的 API 呼叫
   */
  validateDisplayName(displayName: string): { valid: boolean; message?: string } {
    if (!displayName || displayName.trim().length === 0) {
      return { valid: false, message: '顯示名稱不能為空' };
    }
    
    if (displayName.length < 2) {
      return { valid: false, message: '顯示名稱至少需要 2 個字元' };
    }
    
    if (displayName.length > 50) {
      return { valid: false, message: '顯示名稱不能超過 50 個字元' };
    }
    
    // 檢查是否包含特殊字元
    const allowedRegex = /^[a-zA-Z0-9\u4e00-\u9fa5\s\-_]+$/;
    if (!allowedRegex.test(displayName)) {
      return { valid: false, message: '顯示名稱只能包含中英文、數字、空格、連字號和底線' };
    }
    
    return { valid: true };
  }

  /**
   * 驗證密碼強度
   * 前端驗證，提供即時回饋
   */
  validatePassword(password: string): { 
    valid: boolean; 
    strength: 'weak' | 'medium' | 'strong';
    message?: string;
    suggestions?: string[];
  } {
    if (!password) {
      return { 
        valid: false, 
        strength: 'weak',
        message: '密碼不能為空' 
      };
    }
    
    if (password.length < 6) {
      return { 
        valid: false, 
        strength: 'weak',
        message: '密碼至少需要 6 個字元' 
      };
    }
    
    if (password.length > 50) {
      return { 
        valid: false, 
        strength: 'weak',
        message: '密碼不能超過 50 個字元' 
      };
    }
    
    const hasLetter = /[a-zA-Z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    
    const suggestions = [];
    let strength: 'weak' | 'medium' | 'strong' = 'weak';
    
    if (!hasLetter || !hasNumber) {
      return { 
        valid: false, 
        strength: 'weak',
        message: '密碼必須包含至少一個英文字母和一個數字' 
      };
    }
    
    // 計算密碼強度
    let strengthScore = 0;
    if (hasLetter && hasNumber) strengthScore += 2;
    if (hasSpecial) strengthScore += 1;
    if (hasUppercase && hasLowercase) strengthScore += 1;
    if (password.length >= 8) strengthScore += 1;
    
    if (strengthScore >= 4) {
      strength = 'strong';
    } else if (strengthScore >= 3) {
      strength = 'medium';
      if (!hasSpecial) suggestions.push('加入特殊字元可提升密碼強度');
      if (password.length < 8) suggestions.push('使用 8 個以上字元可提升安全性');
    } else {
      strength = 'weak';
      if (!hasSpecial) suggestions.push('建議加入特殊字元');
      if (!hasUppercase) suggestions.push('建議加入大寫字母');
      if (password.length < 8) suggestions.push('建議使用 8 個以上字元');
    }
    
    return { 
      valid: true, 
      strength,
      suggestions: suggestions.length > 0 ? suggestions : undefined
    };
  }

  /**
   * 格式化帳戶統計資料
   * 統一的資料格式化方法
   */
  formatAccountStatistics(stats: any): any {
    return {
      registrationDate: this.formatDate(stats.registrationDate),
      lastLoginDate: this.formatDate(stats.lastLoginDate),
      totalExpenseRecords: stats.totalExpenseRecords || 0,
      totalBudgets: stats.totalBudgets || 0,
      totalCashExpenses: this.formatCurrency(stats.totalCashExpenses || 0),
      totalCreditCardExpenses: this.formatCurrency(stats.totalCreditCardExpenses || 0),
      averageMonthlyExpense: this.formatCurrency(stats.averageMonthlyExpense || 0)
    };
  }

  /**
   * 格式化日期
   * 統一的日期格式化方法
   */
  private formatDate(date: string | Date): string {
    if (!date) return '';
    
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return new Intl.DateTimeFormat('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }).format(dateObj);
  }

  /**
   * 格式化金額
   * 統一的金額格式化方法
   */
  private formatCurrency(amount: number): string {
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: 'TWD',
      minimumFractionDigits: 0
    }).format(amount);
  }
}