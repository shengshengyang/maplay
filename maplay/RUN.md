# 後端執行說明（maplay API）

ASP.NET Core 後端，schema 由 **Flyway** 管理（EF Core 僅作查詢/對應，未啟用 EF Migrations）。

## 一鍵啟動（Docker Compose）

```bash
docker compose up --build
```

啟動順序由 compose 強制：

1. `db`（PostgreSQL 16 + PostGIS）健康後
2. `flyway` 執行 `db/migration/V1–V7`（schema 唯一真實來源）
3. `minio` + `minio-init`（建立公開讀取的 `maplay-images` bucket）
4. `maplay` API：啟動時 `SchemaGuard` 會檢查 `flyway_schema_history`，
   migration 未完成則直接中止啟動（對應 spec「啟動 migration 階段即失敗」）。

服務位置：

- API：http://localhost:8080 （OpenAPI：`/openapi/v1.json`）
- MinIO Console：http://localhost:9001 （minioadmin / minioadmin）
- PostgreSQL：localhost:5432 （maplay / maplay）

## 本機開發（不經 Docker 跑 API）

先以 Docker 起 `db`、`minio`、`flyway`，再本機 `dotnet run`：

```bash
docker compose up -d db minio minio-init flyway
cd maplay && dotnet run
```

連線與金鑰預設值見 `appsettings.Development.json`。

## 設定 admin 帳號

註冊一個一般帳號後，於 DB 將其升級為 admin：

```sql
UPDATE users SET role = 'admin' WHERE email = 'you@example.com';
```

## 架構速覽

| 能力 | 路由前綴 | 檔案 |
|------|----------|------|
| auth | `/api/auth` | `Auth/` |
| spot-management | `/api/spots` | `Spots/` |
| reviews | `/api/spots/{id}/reviews` | `Reviews/` |
| admin-moderation | `/api/admin` | `Admin/` |
| gov-import | `/api/admin/import` | `GovImport/` |
| db-migration | （Flyway） | `db/migration/` |

- 統一回應：成功 `{ success:true, data }`；失敗 `{ success:false, errorCode, message, data }`。
- 例外集中於 `Common/ExceptionHandlingMiddleware`；驗證錯誤統一為 `VALIDATION_ERROR`（`data.errors`）。
- 清單端點強制分頁（`page`/`pageSize`，上限 100）。
- 景點軟刪除（`deleted_at`），公開查詢/詳情/待審查一律排除。
- 圖片經 `IObjectStorage` 抽象（S3 相容，本機接 MinIO）。
- `nearby` 走 PostGIS `ST_DWithin`（geography GIST 函數索引），依距離升冪、附 `distanceMeters`。

## 端點測試

見 `maplay.http`（每個能力都有範例請求）。
