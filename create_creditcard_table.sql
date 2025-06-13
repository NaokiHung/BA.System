-- 創建 CreditCardExpenses 表的 SQL 腳本
-- 適用於 SQLite 數據庫

CREATE TABLE IF NOT EXISTS "CreditCardExpenses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditCardExpenses" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,
    "Amount" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" TEXT NULL,
    "CardName" TEXT NULL,
    "Installments" INTEGER NOT NULL DEFAULT 1,
    "IsOnlineTransaction" INTEGER NOT NULL DEFAULT 0,
    "MerchantName" TEXT NULL,
    "AuthorizationCode" TEXT NULL,
    "CardLastFourDigits" TEXT NULL,
    "CreatedDate" TEXT NOT NULL,
    "UpdatedDate" TEXT NULL,
    "IsBilled" INTEGER NOT NULL DEFAULT 0,
    "BilledDate" TEXT NULL,
    "Notes" TEXT NULL,
    "IsRecurring" INTEGER NOT NULL DEFAULT 0,
    "OriginalAmount" TEXT NULL,
    "OriginalCurrency" TEXT NULL,
    "ExchangeRate" TEXT NULL
);

CREATE INDEX IF NOT EXISTS "IX_CreditCardExpenses_UserId_Year_Month" 
ON "CreditCardExpenses" ("UserId", "Year", "Month");