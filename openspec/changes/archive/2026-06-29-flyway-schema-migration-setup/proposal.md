## Why

目前專案已建立 Flyway 遷移腳本，但需確認各 schema 檔案間的依賴關係正確，並驗證 Flyway 設定與資料庫遷移流程是否符合專案規範。此變更旨在確保資料庫 schema 版本控制機制穩健，避免遷移順序錯誤導致的部署失敗。

## What Changes

- 檢查現有 Flyway 遷移檔案的依賴關係 (V1-V7)
- 驗證遷移順序是否符合 foreign key 依賴
- 確認 Flyway 設定檔配置正確性
- 建立遷移執行與驗證程序

## Capabilities

### New Capabilities
- `db-migration`: 資料庫遷移機制，包含 Flyway 設定、遷移順序驗證、版本控制與執行流程

### Modified Capabilities
(無 - 此變更主要為驗證與設定確認，不修改既有能力需求)

## Impact

**受影響系統：**
- 資料庫層：PostgreSQL 16 + PostGIS schema 結構
- 部署流程：Docker Compose 啟動時的 Flyway 自動遷移
- 開發流程：新增 schema 時需遵循 Flyway 命名與依賴規範

**風險評估：**
- 低風險：僅驗證與確認現有設定，不修改既有 schema
- 若發現依賴順序錯誤，需調整遷移檔案版本號

## Non-goals (本期不做)

- 不修改既有 schema 欄位定義
- 不新增額外的資料表或欄位
- 不變更 EF Core 與 Flyway 的權責劃分
- 不涉及資料遷移或轉換