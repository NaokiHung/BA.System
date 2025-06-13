/**
 * 檔案路徑: budget-assistant-web/src/app/core/services/auth.service.ts
 * 最終修正版本的 AuthService，解決 displayName 問題
 */

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, LoginResponse, User } from '../models/auth.models';

/**
 * 認證服務
 * 完整處理 JWT Token 中的所有 claims，包括 displayName
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private tokenKey = environment.tokenKey;
  
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // 應用程式啟動時檢查是否已有有效的 Token
    this.checkStoredToken();
  }

  /**
   * 使用者登入
   */
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request)
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.handleAuthenticationSuccess(response);
          }
        })
      );
  }

  /**
   * 使用者註冊
   */
  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/register`, request);
  }

  /**
   * 檢查使用者名稱是否可用
   */
  checkUsername(username: string): Observable<{ available: boolean }> {
    return this.http.get<{ available: boolean }>(`${this.apiUrl}/auth/check-username/${username}`);
  }

  /**
   * 登出處理
   */
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/auth/login']);
  }

  /**
   * 獲取當前儲存的 Token
   */
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * 檢查 Token 是否有效
   */
  isTokenValid(): boolean {
    const token = this.getToken();
    if (!token) {
      console.log('🔍 Token 檢查: 沒有 Token');
      return false;
    }

    try {
      // 解析 JWT Token 檢查過期時間
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      
      console.log('🔍 Token 檢查:', {
        expires: new Date(payload.exp * 1000),
        current: new Date(),
        isValid: payload.exp > currentTime,
        payload: payload // 顯示完整的 payload 進行除錯
      });
      
      return payload.exp > currentTime;
    } catch (error) {
      console.error('❌ Token 解析失敗:', error);
      return false;
    }
  }

  /**
   * 處理認證成功
   */
  private handleAuthenticationSuccess(response: LoginResponse): void {
    // 儲存 Token
    localStorage.setItem(this.tokenKey, response.token!);
    
    // 從 Token 中解析完整的使用者資訊
    const user = this.getUserFromToken(response.token!);
    
    if (user) {
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
      console.log('✅ 認證成功，使用者:', user);
    } else {
      console.error('❌ 無法從 Token 中解析使用者資訊');
    }
  }

  /**
   * 檢查本地儲存的 Token 並初始化認證狀態
   */
  private checkStoredToken(): void {
    console.log('🔍 檢查儲存的 Token...');
    
    const token = this.getToken();
    
    if (!token) {
      console.log('❌ 沒有找到 Token');
      this.isAuthenticatedSubject.next(false);
      this.currentUserSubject.next(null);
      return;
    }

    console.log('✅ 找到 Token，正在驗證...');
    
    // 檢查 Token 是否有效
    if (this.isTokenValid()) {
      console.log('✅ Token 有效，正在解析使用者資訊...');
      
      try {
        // 從 Token 中解析使用者資訊
        const user = this.getUserFromToken(token);
        if (user) {
          console.log('✅ 使用者資訊解析成功:', user);
          this.currentUserSubject.next(user);
          this.isAuthenticatedSubject.next(true);
        } else {
          console.log('❌ 無法解析使用者資訊');
          this.clearAuthenticationState();
        }
      } catch (error) {
        console.error('❌ 解析 Token 時發生錯誤:', error);
        this.clearAuthenticationState();
      }
    } else {
      console.log('❌ Token 已過期或無效');
      this.clearAuthenticationState();
    }
  }

  /**
   * 從 Token 中解析使用者資訊
   * 處理後端 JWT Token 中的所有可能的 claim 名稱
   */
  private getUserFromToken(token: string): User | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      console.log('🔍 Token payload:', payload); // 除錯用
      
      // 後端使用的 claim 名稱對應：
      // ClaimTypes.NameIdentifier -> "nameid" 或 "sub"
      // ClaimTypes.Name -> "unique_name" 或 "name"  
      // "DisplayName" -> "DisplayName"
      
      const user: User = {
        id: payload.nameid || payload.sub || payload.userId || '',
        username: payload.unique_name || payload.name || payload.username || '',
        email: payload.email || '',
        displayName: payload.DisplayName || payload.displayName || payload.unique_name || payload.name || payload.username || 'User'
      };
      
      // 驗證必要欄位
      if (!user.id || !user.username) {
        console.error('❌ Token 中缺少必要的使用者資訊:', payload);
        return null;
      }
      
      return user;
    } catch (error) {
      console.error('❌ 解析 Token payload 失敗:', error);
      return null;
    }
  }

  /**
   * 更新當前使用者資訊
   * 用於個人資料更新後同步全域狀態
   */
  updateCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
    console.log('✅ 使用者資訊已更新:', user);
  }

  /**
   * 清除認證狀態
   */
  private clearAuthenticationState(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }
}