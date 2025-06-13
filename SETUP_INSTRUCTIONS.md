# Budget Assistant 設置說明

## 後端設置

### 1. 數據庫遷移
為了支持信用卡支出功能，需要創建新的數據庫表。

#### 方法一：使用 Entity Framework 遷移（推薦）
```bash
cd BA.Server
dotnet ef database update --context ExpenseDbContext
```

#### 方法二：手動執行 SQL（如果遷移失敗）
如果自動遷移失敗，可以手動運行 SQL 腳本：

1. 打開 SQLite 數據庫文件：`BA.Server/ExpenseDb.db`
2. 執行 `create_creditcard_table.sql` 中的 SQL 語句

### 2. 編譯錯誤修正
已修正的問題：
- ✅ `CS0019` 錯誤：修正了 `Installments` 屬性的類型問題
- ✅ `CS8602` 警告：修正了可能的 null 參考問題

### 3. 啟動後端
```bash
cd BA.Server
dotnet run
```

## 前端設置

### 1. 安裝依賴（如果尚未安裝）
```bash
cd budget-assistant-web
npm install
```

### 2. 啟動前端
```bash
cd budget-assistant-web
ng serve
```

## 新功能

### ✅ 已實現的功能：
1. **信用卡支出管理**
   - 新增信用卡支出記錄
   - 查看信用卡支出統計
   - 信用卡支出不影響現金預算

2. **現代化 UI 設計**
   - 漸變背景和玻璃形態效果
   - 響應式卡片布局
   - 流暢的動畫效果

3. **預算計算修正**
   - 修正了預算使用率計算邏輯
   - 添加了詳細的調試日誌

### 🔧 如果遇到問題：

1. **後端編譯錯誤**：確保已應用所有代碼修正
2. **數據庫錯誤**：確保已運行數據庫遷移
3. **前端連接錯誤**：確保後端服務正在運行（通常在 http://localhost:5091）

## 測試步驟

1. 啟動後端和前端服務
2. 登入系統
3. 設定月預算
4. 新增現金支出測試
5. 新增信用卡支出測試
6. 查看儀表板統計數據
7. 檢查預算使用率計算是否正確