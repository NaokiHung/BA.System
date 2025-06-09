import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, LoginResponse, User } from '../models/auth.models';

/**
 * 認證服務
 * 為什麼使用 @Injectable({ providedIn: 'root' })？
 * 1. 創建全域單例服務
 * 2. 自動在根注入器中註冊
 * 3. 避免重複創建實例
 * 4. 所有組件共享相同的認證狀態
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private tokenKey = environment.tokenKey;
  
  /**
   * 使用 BehaviorSubject 管理認證狀態
   * 為什麼選擇 BehaviorSubject？
   * 1. 有初始值，訂閱時立即獲得當前狀態
   * 2. 支援多個組件同時訂閱
   * 3. 狀態變更時自動通知所有訂閱者
   */
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
   * 與後端 AuthController.Login 對應
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
   * 與後端 AuthController.Register 對應
   */
  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/register`, request);
  }

  /**
   * 檢查使用者名稱是否可用
   * 與後端 AuthController.CheckUsername 對應
   */
  checkUsername(username: string): Observable<{ available: boolean }> {
    return this.http.get<{ available: boolean }>(`${this.apiUrl}/auth/check-username/${username}`);
  }

  /**
   * 登出處理
   */
  logout(): void {
    // 清除本地儲存的認證資訊
    localStorage.removeItem(this.tokenKey);
    
    // 重置認證狀態
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    
    // 導向登入頁面
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
    if (!token) return false;

    try {
      // 解析 JWT Token 檢查過期時間
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expirationDate = new Date(payload.exp * 1000);
      return expirationDate > new Date();
    } catch {
      return false;
    }
  }

  /**
   * 處理認證成功後的邏輯
   */
  private handleAuthenticationSuccess(response: LoginResponse): void {
    if (response.token) {
      // 儲存 Token 到 localStorage
      localStorage.setItem(this.tokenKey, response.token);
      
      // 更新使用者資訊
      const user: User = {
        id: response.userId!,
        username: response.username!,
        displayName: response.username! // 如果後端沒有回傳 displayName，使用 username
      };
      
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    }
  }

  /**
   * 檢查本地儲存的 Token
   * 應用程式啟動時調用，恢復認證狀態
   */
  private checkStoredToken(): void {
    if (this.isTokenValid()) {
      const token = this.getToken()!;
      try {
        // 從 Token 中解析使用者資訊
        const payload = JSON.parse(atob(token.split('.')[1]));
        const user: User = {
          id: payload.nameid || payload.sub,
          username: payload.unique_name || payload.name,
          displayName: payload.DisplayName || payload.unique_name || payload.name
        };
        
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
      } catch (error) {
        // Token 格式錯誤，清除無效 Token
        this.logout();
      }
    }
  }
}