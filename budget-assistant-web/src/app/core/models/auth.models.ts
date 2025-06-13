/**
 * 認證相關的資料模型
 * 修正：將 User 模型移動到這裡
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