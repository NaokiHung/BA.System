/**
 * 認證相關的資料模型
 * 為什麼要定義 TypeScript 介面？
 * 1. 提供型別安全性
 * 2. 自動完成和智能提示
 * 3. 編譯時錯誤檢查
 * 4. 與後端 DTO 保持一致
 */

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  confirmPassword: string;
  email?: string;
  displayName?: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  userId?: string;
  username?: string;
  expiresAt?: Date;
}

export interface User {
  id: string;
  username: string;
  email?: string;
  displayName: string;
}