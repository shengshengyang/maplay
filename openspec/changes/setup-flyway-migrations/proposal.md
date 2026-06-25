## Why

目前專案缺乏資料庫結構版本控制與依賴關係檢查機制。需要在專案啟動初期建立標準化的資料庫遷移流程，確保 schema 變更可追蹤、可重現，並避免表/欄位依賴衝突。

## What Changes

- 新增 Flyway 作為資料庫遷移工具，管理所有 schema 版本
- 建立標準化的 SQL 遷移腳本結構，包含表、索引、外鍵、約束條件
- 實作依賴關係檢查機制，確保表與表之間的依賴順序正確
- 整合 Flyway 到 ASP.NET Core 啟動流程，應用程式啟動時自動執行待處理遷移
- **BREAKING**: 禁用 EF Core Migrations，schema 擁有權完全轉移至 Flyway

## Capabilities

### New Capabilities
- `db-migration`: 資料庫結構版本化管理與遷移執行，包含依賴關係驗證

### Modified Capabilities
- (無) - 此變更為新增能力，不修改既有能力需求

## Impact

**受影響系統**：
- PostgreSQL 資料庫結構（所有 DDL 操作改由 Flyway 管理）
- ASP.NET Core 啟動流程（新增 Flyway 自動執行邏輯）
- EF Core 配置（僅保留查詢/對應功能，移除遷移能力）

**依賴套件**：
- 新增 Flyway CLI 或 Flyway .NET 整合套件
- 確保 PostgreSQL 連線資訊可用於遷移執行

**風險**：
- 若現有資料庫已存在結構，需先建立 baseline 遷移版本
- 開發人員需遵守 Flyway 遷移命名規範與依賴順序規則

## Non-goals (本期不做)

- 不處理資料遷移（Data Migration），僅處理結構遷移（Schema Migration）
- 不建立自動化回滾機制（初期依賴手動管理 -down 腳本）
- 不整合進 CI/CD 流程（本期專注於本機開發環境）