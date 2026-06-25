## ADDED Requirements

### Requirement: 跨 capability 完整 schema 覆蓋
Flyway migration 腳本 SHALL 涵蓋所有 capability spec 所定義的資料表與欄位。每張表的所有欄位（包含來自多個 spec 的欄位）MUST 集中於同一版本的 migration 中定義，不得分散導致外鍵約束無法建立。

> **跨 spec 欄位歸屬說明（spots 表為重點）：**
> - `spot-management` spec 定義 spots 基礎欄位（name, category, status, spot_type, start/end_date, age_groups, facilities, location, submitted_by, source, gov_data_id, deleted_at）
> - `admin-moderation` spec 新增 reviewed_by, reviewed_at, reject_reason
> - `gov-import` spec 使用 gov_data_id（UNIQUE）與 source 欄位
> - 以上三個 spec 的欄位 MUST 全部出現在同一個 `V__create_spots.sql` migration 中

#### Scenario: spots 表完整欄位驗證
- **WHEN** 執行 spots 表 migration
- **THEN** 以下欄位 MUST 全部存在：id, name, category, status, spot_type, start_date, end_date, age_groups, facilities, location, submitted_by, source, gov_data_id, reviewed_by, reviewed_at, reject_reason, deleted_at, created_at, updated_at
- **AND** `gov_data_id` SHALL 有 UNIQUE 部分索引（僅對非 NULL 值）

#### Scenario: 遺漏跨 spec 欄位導致功能失效
- **WHEN** admin-moderation 或 gov-import 功能啟動
- **AND** spots 表缺少對應欄位
- **THEN** 應用程式 SHALL 在啟動 migration 階段即失敗，而非在執行期拋出欄位不存在錯誤

### Requirement: refresh_tokens 表定義
系統 SHALL 建立 `refresh_tokens` 表以支援 auth capability 的 token 撤銷需求。此表雖未在 auth spec 中明確列出，但 MUST 存在以實現 refreshToken 輪替與登出後失效行為。

#### Scenario: refresh_tokens 表必要欄位
- **WHEN** 執行 refresh_tokens migration
- **THEN** 以下欄位 MUST 存在：id (UUID PK), user_id (UUID FK → users), token_hash (VARCHAR UNIQUE), expires_at (TIMESTAMPTZ NOT NULL), revoked_at (TIMESTAMPTZ nullable), created_at
- **AND** user_id FK SHALL ON DELETE CASCADE

#### Scenario: token 撤銷查詢
- **WHEN** 查詢某 token 是否有效
- **THEN** 系統 SHALL 以 token_hash 欄位查詢，MUST 有 UNIQUE 索引保障查詢效能

### Requirement: users.email nullable 與唯一約束共存
系統 SHALL 允許 `users.email` 為 NULL（支援 Line OAuth 無 email 情境），同時維持已提供 email 的唯一性。UNIQUE 約束 MUST 以「部分唯一索引」實現，僅約束非 NULL 的 email 值。

#### Scenario: Line OAuth 無 email 可建立帳號
- **WHEN** 建立 provider=line 且 email=NULL 的使用者
- **THEN** 建立成功，不違反 UNIQUE 約束
- **AND** 多筆 email=NULL 的使用者可同時存在

#### Scenario: 相同 email 不可重複
- **WHEN** 建立第二筆相同非 NULL email 的使用者
- **THEN** UNIQUE 約束觸發，資料庫回傳違反唯一性錯誤

### Requirement: age_groups 與 facilities 型別統一
系統 SHALL 以 `TEXT[]`（PostgreSQL 原生陣列）儲存 spots 表的 `age_groups` 與 `facilities` 欄位，值域由應用層驗證（非資料庫 CHECK constraint），允許未來擴充類別而無需 migration。

#### Scenario: age_groups 合法值儲存
- **WHEN** 儲存 age_groups = ['0-3', '3-7']
- **THEN** 資料庫 MUST 正確儲存為 TEXT 陣列

#### Scenario: facilities 空陣列
- **WHEN** 儲存 facilities = []（無設施標籤）
- **THEN** 欄位儲存為空陣列 `'{}'`，NOT NULL，允許空但不允許 NULL

### Requirement: Migration 執行順序與 FK 依賴
系統 MUST 依下列順序執行 migration 以確保外鍵約束可正確建立。任何違反此順序的 migration 腳本 SHALL 導致 migration 失敗並回滾。

**強制執行順序：**
1. `V1__enable_postgis.sql` — 啟用 PostGIS extension（無 FK 依賴）
2. `V2__create_users.sql` — users 表（無 FK 依賴）
3. `V3__create_refresh_tokens.sql` — refresh_tokens 表（FK → users）
4. `V4__create_spots.sql` — spots 表含全部跨 spec 欄位（FK → users）
5. `V5__create_spot_images.sql` — spot_images 表（FK → spots, users）
6. `V6__create_reviews.sql` — reviews 表（FK → spots, users）
7. `V7__create_import_history.sql` — import_history 表（FK → users）

#### Scenario: 正向 — 空庫完整 migrate
- **WHEN** 對空資料庫依序執行 V1–V7
- **THEN** 所有表與索引建立成功，flyway_schema_history 記錄 7 筆

#### Scenario: 違反順序 — FK 目標表不存在
- **WHEN** 嘗試在 V2__create_users 之前執行含 user_id FK 的 migration
- **THEN** PostgreSQL 拋出外鍵約束錯誤
- **AND** Flyway 標記該 migration 失敗並中止後續執行

#### Scenario: 必要 PostGIS index
- **WHEN** V4__create_spots 執行完畢
- **THEN** `location` 欄位 SHALL 建立 GIST 空間索引
- **AND** `status`, `deleted_at`, `spot_type`, `end_date` 欄位 SHALL 建立複合索引以支援 nearby 查詢過濾

### Requirement: Flyway manages all database schema changes
The system SHALL use Flyway as the single source of truth for all database schema modifications. Every structural change MUST be versioned through Flyway migration scripts.

#### Scenario: Initial schema creation
- **WHEN** application starts for the first time on an empty database
- **THEN** Flyway SHALL execute all migration scripts in version order
- **AND** database schema SHALL match the latest migration version

#### Scenario: Incremental schema updates
- **WHEN** new migration files are added to the migrations directory
- **AND** application starts
- **THEN** Flyway SHALL execute only pending migrations
- **AND** flyway_schema_history table SHALL record all executed migrations

#### Scenario: Migration execution failure
- **WHEN** a migration script fails during execution
- **THEN** Flyway SHALL mark the migration as failed
- **AND** application startup SHALL fail with clear error message
- **AND** database SHALL remain in consistent state

### Requirement: Migration scripts enforce dependency order
The system SHALL validate that database objects are created in correct dependency order. Tables referenced by foreign keys MUST exist before constraints are applied.

#### Scenario: Table dependency validation
- **WHEN** a migration creates a table with foreign key to another table
- **THEN** the referenced table MUST exist in the same or earlier migration
- **AND** foreign key creation SHALL succeed

#### Scenario: Index creation order
- **WHEN** a migration creates an index on a column
- **THEN** the table and column MUST exist in the same or earlier migration
- **AND** index creation SHALL succeed

#### Scenario: Circular dependency detection
- **WHEN** migration scripts would create circular dependencies
- **THEN** migration execution SHALL fail
- **AND** error message SHALL indicate the circular dependency

### Requirement: EF Core migrations are disabled
The system SHALL NOT use EF Core migrations for schema management. EF Core MUST be configured for query and entity mapping only.

#### Scenario: EF Core query-only configuration
- **WHEN** EF Core is configured in application startup
- **THEN** migrations SHALL be explicitly disabled
- **AND** DbContext SHALL be usable for queries only

#### Scenario: Attempted EF Core migration blocked
- **WHEN** developer attempts to run EF Core migration commands
- **THEN** the command SHALL fail or produce warning
- **AND** developer SHALL be directed to use Flyway instead

### Requirement: Automatic migration execution on startup
The system SHALL automatically execute pending Flyway migrations during ASP.NET Core application startup.

#### Scenario: Pending migrations detected
- **WHEN** application starts
- **AND** pending migrations exist in database
- **THEN** Flyway SHALL automatically execute pending migrations
- **AND** application SHALL continue startup after migrations complete

#### Scenario: No pending migrations
- **WHEN** application starts
- **AND** database schema is current
- **THEN** Flyway SHALL skip migration execution
- **AND** application SHALL continue normal startup

#### Scenario: Migration timeout handling
- **WHEN** a migration execution exceeds configured timeout
- **THEN** migration execution SHALL be terminated
- **AND** application startup SHALL fail
- **AND** error SHALL indicate which migration timed out

### Requirement: Migration file naming conventions
The system SHALL enforce Flyway-standard naming conventions for migration SQL scripts.

#### Scenario: Versioned migration naming
- **WHEN** developer creates a new migration file
- **THEN** filename MUST follow pattern V{version}__{description}.sql
- **AND** version MUST be sequential numeric
- **AND** description MUST use snake_case

#### Scenario: Repeatable migration naming
- **WHEN** developer creates a repeatable migration
- **THEN** filename MUST follow pattern R__{description}.sql
- **AND** migration SHALL be re-applied when checksum changes

#### Scenario: Invalid naming rejected
- **WHEN** migration filename does not follow conventions
- **THEN** Flyway SHALL reject the file
- **AND** error SHALL indicate naming convention violation

### Requirement: Schema baseline for existing databases
The system SHALL support establishing Flyway baseline for databases with existing schema.

#### Scenario: Baseline creation
- **WHEN** Flyway is initialized on existing database
- **THEN** system SHALL create baseline migration representing current schema
- **AND** flyway_schema_history SHALL record baseline
- **AND** subsequent migrations SHALL be applied normally

#### Scenario: Baseline validation
- **WHEN** baseline is created
- **THEN** system SHA validate schema matches expected baseline state
- **AND** any mismatch SHALL cause baseline to fail

### Requirement: Migration rollback capability
The system SHALL support manual rollback through explicit migration scripts.

#### Scenario: Manual rollback migration
- **WHEN** developer creates rollback script following naming convention
- **THEN** rollback SHALL be executable as new migration
- **AND** rollback SHALL revert specific schema changes

#### Scenario: Rollback execution
- **WHEN** rollback migration is executed
- **THEN** affected schema objects SHALL be reverted to previous state
- **AND** flyway_schema_history SHALL record rollback migration
