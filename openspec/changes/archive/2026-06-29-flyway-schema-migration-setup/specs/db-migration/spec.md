## ADDED Requirements

### Requirement: 資料庫遷移版本控制
系統 SHALL 使用 Flyway 進行資料庫 schema 版本控制，所有 schema 變更必須通過版本化 SQL 遷移檔案管理。

#### Scenario: Flyway 遷移檔案版本命名
- **WHEN** 開發者建立新的遷移檔案
- **THEN** 檔案名稱遵循 `V{version}__{description}.sql` 格式
- **AND** 版本號為遞增整數 (V1, V2, V3...)
- **AND** 描述使用小寫與底線 (例如: create_users_table)

#### Scenario: 遷移檔案儲存位置
- **WHEN** 儲存 Flyway 遷移檔案
- **THEN** 檔案位於 `db/migration/` 目錄
- **AND** 檔案使用 `.sql` 副檔名

### Requirement: 遷移依賴順序驗證
系統 SHALL 確保遷移檔案執行順序符合資料表之間的 foreign key 依賴關係。

#### Scenario: 正向依賴順序執行
- **WHEN** Flyway 執行遷移
- **THEN** 依賴基礎資料表的遷移先執行
- **AND** 被依賴的資料表遷移後執行
- **AND** foreign key constraint 建立成功

#### Scenario: 依賴順序錯誤檢測
- **WHEN** 遷移檔案順序錯誤
- **THEN** Flyway 執行時拋出錯誤
- **AND** 錯誤訊息包含 foreign key 不存在的資訊
- **AND** 開發者需調整版本號或修正依賴關係

### Requirement: Flyway 設定檔配置
系統 SHALL 提供正確的 Flyway 配置，包含資料庫連線與遷移路徑設定。

#### Scenario: 開發環境配置
- **WHEN** 應用程式啟動於開發環境
- **THEN** Flyway 連線至本地 PostgreSQL 資料庫
- **AND** 遷移檔案路徑指向 `db/migration/` 目錄
- **AND** 自動執行待執行的遷移

#### Scenario: Docker 容器內配置
- **WHEN** 應用程式於 Docker 容器內啟動
- **THEN** Flyway 連線至容器內 PostgreSQL 服務
- **AND** 等待資料庫服務就緒後執行遷移
- **AND** 記錄遷移執行結果

### Requirement: 遷移執行與驗證
系統 SHALL 提供遷移執行狀態檢查與驗證機制，確保資料庫 schema 與遷移檔案同步。

#### Scenario: 遷移成功驗證
- **WHEN** Flyway 完成遷移執行
- **THEN** 記錄執行的遷移版本與時間戳記
- **AND** `flyway_schema_history` 表包含執行記錄
- **AND** 回傳成功狀態碼

#### Scenario: 遷移失敗回滾
- **WHEN** 遷移執行過程中發生錯誤
- **THEN** Flyway 停止後續遷移執行
- **AND** 記錄失敗的遷移版本與錯誤訊息
- **AND** 資料庫保持在錯誤發生前的狀態

#### Scenario: 現有資料庫驗證
- **WHEN** 連線至已有資料的資料庫
- **THEN** Flyway 檢查 `flyway_schema_history` 表
- **AND** 僅執行未執行過的新遷移
- **AND** 跳過已執行過的遷移版本

### Requirement: Schema 唯一真實來源
系統 SHALL 以 Flyway 遷移檔案為資料庫 schema 的唯一真實來源，禁止使用其他機制修改 schema。

#### Scenario: EF Core 僅用於查詢
- **WHEN** 應用程式使用 EF Core
- **THEN** EF Core 僅用於查詢與物件對應
- **AND** EF Core Migrations 功能保持停用
- **AND** 所有 schema 變更通過 Flyway 管理

#### Scenario: 手動 SQL 修改禁止
- **WHEN** 開發者需要修改資料表結構
- **THEN** 必須建立新的 Flyway 遷移檔案
- **AND** 禁止直接執行 DDL 修改 production 資料庫
- **AND** 所有變更需通過版本控制審核