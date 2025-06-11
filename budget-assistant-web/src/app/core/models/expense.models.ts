/**
 * 支出管理相關的資料模型 - 增強版本
 * 對應後端的 DTOs，新增信用卡支出和編輯功能
 */

// === 支出類型枚舉 ===
export enum ExpenseType {
  Cash = 'Cash',           // 現金支出
  CreditCard = 'CreditCard' // 信用卡支出
}

// === 請求模型 ===

export interface AddCashExpenseRequest {
  amount: number;
  description: string;
  category?: string;
}

/**
 * 新增信用卡支出請求
 * 為什麼要獨立信用卡支出模型？
 * 1. 信用卡支出不會立即影響現金預算
 * 2. 可能需要記錄信用卡資訊（如卡號後四碼）
 * 3. 便於未來擴展信用卡相關功能
 */
export interface AddCreditCardExpenseRequest {
  amount: number;
  description: string;
  category?: string;
  cardName?: string;        // 信用卡名稱（如：中信紅利卡）
  installments?: number;    // 分期期數（預設為1，表示一次付清）
}

/**
 * 更新支出記錄請求
 * 為什麼需要 expenseType？
 * 避免將現金支出誤改為信用卡支出，保持資料一致性
 */
export interface UpdateExpenseRequest {
  amount: number;
  description: string;
  category?: string;
  expenseType: ExpenseType; // 確保不會意外改變支出類型
  cardName?: string;        // 僅用於信用卡支出
  installments?: number;    // 僅用於信用卡支出
}

export interface SetBudgetRequest {
  amount: number;
  year: number;
  month: number;
}

// === 回應模型 ===

export interface ExpenseResponse {
  success: boolean;
  message: string;
  expenseId?: number;
  remainingBudget: number;  // 剩餘現金預算（信用卡支出不影響此值）
}

export interface MonthlyBudgetResponse {
  totalBudget: number;           // 當月預算總額
  remainingCash: number;         // 剩餘現金預算
  totalCashExpenses: number;     // 已支出現金總額
  totalSubscriptions: number;    // 訂閱服務總額
  totalCreditCard: number;       // 信用卡消費總額
  combinedCreditTotal: number;   // 信用卡+訂閱總額
  year: number;
  month: number;
  monthName: string;             // 顯示用的月份名稱
}

/**
 * 支出歷史記錄 - 增強版本
 * 新增支出類型和相關欄位
 */
export interface ExpenseHistory {
  id: number;
  amount: number;
  description: string;
  category: string;
  date: string;
  expenseType: ExpenseType;      // 支出類型
  cardName?: string;             // 信用卡名稱（僅信用卡支出）
  installments?: number;         // 分期期數（僅信用卡支出）
  canEdit: boolean;              // 是否可編輯（如：當月記錄可編輯）
  canDelete: boolean;            // 是否可刪除
}

/**
 * 支出記錄詳情回應
 * 用於編輯功能的資料載入
 */
export interface ExpenseDetailResponse {
  id: number;
  amount: number;
  description: string;
  category: string;
  expenseType: ExpenseType;
  cardName?: string;
  installments?: number;
  createdDate: string;
  year: number;
  month: number;
}

// === 使用者相關模型 ===

export interface User {
  id: string;
  username: string;
  displayName: string;
  email?: string;              // 新增 email 欄位
}

/**
 * 使用者資料更新請求
 * 為什麼要分開處理個人資料和密碼？
 * 1. 安全考量：密碼變更需要額外驗證
 * 2. 使用頻率不同：個人資料可能經常調整，密碼較少變更
 * 3. 權限管理：某些欄位可能有不同的權限要求
 */
export interface UpdateUserProfileRequest {
  displayName: string;
  email: string;
}

/**
 * 密碼變更請求
 * 為什麼需要舊密碼？
 * 安全考量，確認是本人操作
 */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * 使用者資料更新回應
 */
export interface UserProfileResponse {
  success: boolean;
  message: string;
  user?: User;
}

// === 前端專用的介面模型 ===

/**
 * 支出記錄表格顯示模型
 * 為什麼要獨立前端顯示模型？
 * 1. 加入前端專用的格式化欄位
 * 2. 避免直接修改 API 回應模型
 * 3. 便於前端邏輯處理
 */
export interface ExpenseTableItem extends ExpenseHistory {
  formattedAmount: string;      // 格式化後的金額顯示
  formattedDate: string;        // 格式化後的日期顯示
  typeDisplayName: string;      // 支出類型顯示名稱
  isEditing?: boolean;          // 是否正在編輯中
}

/**
 * 支出統計資料
 * 用於儀表板圖表和分析顯示
 */
export interface ExpenseStatistics {
  totalCashExpenses: number;
  totalCreditCardExpenses: number;
  categoryBreakdown: CategoryExpense[];
  monthlyTrend: MonthlyExpense[];
}

/**
 * 類別支出統計
 */
export interface CategoryExpense {
  category: string;
  amount: number;
  count: number;
  percentage: number;
}

/**
 * 月度支出統計
 */
export interface MonthlyExpense {
  year: number;
  month: number;
  monthName: string;
  cashAmount: number;
  creditCardAmount: number;
  totalAmount: number;
}

// === 表單驗證相關 ===

/**
 * 表單驗證錯誤訊息
 */
export interface FormValidationErrors {
  [key: string]: string;
}

/**
 * 支出表單狀態
 */
export interface ExpenseFormState {
  isLoading: boolean;
  errors: FormValidationErrors;
  isDirty: boolean;
}