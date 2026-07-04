# 親子資源地圖系統 (Family Resource Map)

整合公園、親子餐廳、哺乳室廁所、活動、醫療等親子資源地圖應用，支援使用者回報、管理員審查、政府開放資料匯入、Google Maps 導航跳轉。

## 專案概述

**技術架構:**
- 🌐 **前端**: Vue 3 + Vite + Pinia + Vue Router + Leaflet.js
- ⚙️ **後端**: ASP.NET Core 10 Web API
- 🗄️ **資料庫**: PostgreSQL 16 + PostGIS
- 🔄 **遷移**: Flyway 版本化 SQL (schema 唯一來源)
- 🔐 **認證**: JWT + OAuth (Google/Line)
- 📚 **文檔**: .NET 原生 OpenAPI + Scalar UI
- 📦 **部署**: Docker Compose
- 💾 **儲存**: S3 相容物件儲存 (MinIO 本機開發)

## 快速開始

### 前置要求

- Docker Desktop (最新版)
- .NET 8 SDK (本地開發)
- Node.js 18+ (前端開發)

### 一鍵啟動

```bash
# 複製環境變數檔案
cp .env.example .env

# 啟動所有服務
docker compose up -d

# 查看服務狀態
docker compose ps

# 查看日誌
docker compose logs -f
```

**服務包含:**
- PostgreSQL 16 + PostGIS (port 5432)
- Flyway 資料庫遷移
- MinIO 物件儲存 (port 9000, 9001)
- Web API (port 8080)

### 訪問服務

- **Swagger UI** (推薦): http://localhost:8080/swagger (穩定的互動式 API 文檔)
- **Scalar UI**: http://localhost:8080/scalar/v1 (現代化 API 文檔介面)
- **OpenAPI JSON**: http://localhost:8080/openapi/v1.json
- **MinIO Console**: http://localhost:9001 (minioadmin/minioadmin)
- **PostgreSQL**: localhost:5432 (maplay/maplay)

### 快速測試

```bash
# 使用 PowerShell 測試腳本
.\test-api-fixed.ps1

# 或手動測試 API
Invoke-WebRequest http://localhost:8080/openapi/v1.json
```

## 資料庫遷移設定

### 遷移系統架構

本專案使用 **Flyway** 作為資料庫 schema 版本控制工具：

- **Schema 擁有權**: 歸 Flyway 管理
- **EF Core 角色**: 僅用於查詢與物件對應
- **遷移檔案**: 版本化 SQL，存於 `db/migration/`

### 現有遷移檔案

```
db/migration/
├── V1__enable_postgis.sql              # PostGIS + pgcrypto extensions
├── V2__create_users.sql                 # 使用者表
├── V3__create_refresh_tokens.sql        # 更新權杖表
├── V4__create_spots.sql                 # 景點表
├── V5__create_spot_images.sql           # 景點圖片表
├── V6__create_reviews.sql               # 評價表
└── V7__create_import_history.sql        # 匯入歷史表
```

### Flyway 設定檔

專案根目錄的 `flyway.conf` 包含遷移配置：

```ini
# 資料庫連線
flyway.url=jdbc:postgresql://localhost:5432/maplay
flyway.user=postgres
flyway.password=postgres

# 遷移檔案位置
flyway.locations=filesystem:db/migration

# 遷移行為
flyway.validate-on-migrate=true
flyway.baseline-on-migrate=true
```

**環境變數覆蓋**: 可透過 `.env` 檔案或環境變數覆蓋設定值。

### 執行遷移

#### Docker 環境 (推薦)

```bash
# Flyway 在 Docker Compose 中自動執行
docker compose up -d flyway

# 查看遷移執行結果
docker compose logs flyway

# 檢查遷移狀態
docker compose exec flyway flyway info
```

#### 本地開發環境

```bash
# 安裝 Flyway CLI
# macOS: brew install flyway
# Windows: chocolatey install flyway
# Linux: 下載從 flywaydb.org

# 設定環境變數
export FLYWAY_URL=jdbc:postgresql://localhost:5432/maplay
export FLYWAY_USER=maplay
export FLYWAY_PASSWORD=maplay

# 執行遷移
flyway migrate

# 驗證狀態
flyway info
flyway validate
```

### 新增遷移檔案

#### 1. 命名規則

```
V{version}__{description}.sql
```

**範例:**
- `V8__create_user_profiles.sql`
- `V9__add_status_to_reviews.sql`
- `V10__create_bookings_table.sql`

#### 2. 基本結構

```sql
-- V8: user_profiles 表（FK → users）
-- 用途：儲存使用者詳細資料與偏好設定

CREATE TABLE user_profiles (
    id         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID        NOT NULL,
    bio        TEXT,
    preferences JSONB     DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_user_profiles_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_profiles_user_id ON user_profiles (user_id);
```

#### 3. 依賴檢查

- **FK 依賴**: 確保引用的資料表在先前版本建立
- **版本順序**: 遞增整數，禁止跳號
- **循環檢查**: 避免循環依賴

#### 4. 測試遷移

```bash
# 清理測試環境
docker compose down -v

# 完整測試
docker compose up -d

# 檢查結果
docker compose logs flyway
docker compose exec flyway flyway info

# 驗證資料表
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "\dt"
```

### 相關文件

📚 **詳細資源**:
- **API 文檔指南**: [OPENAPI_GUIDE.md](./OPENAPI_GUIDE.md)
- **Docker 指令**: [DOCKER_COMMANDS.md](./DOCKER_COMMANDS.md)
- **部署說明**: [DOCKER_DEPLOYMENT.md](./DOCKER_DEPLOYMENT.md)
- **開發者手冊**: [DEVELOPER_GUIDE.md](./DEVELOPER_GUIDE.md)
- **遷移工作流程**: `openspec/changes/flyway-schema-migration-setup/migration-workflow-guide.md`
- **命名規則**: `openspec/changes/flyway-schema-migration-setup/migration-naming-dependency-rules.md`
- **錯誤排查**: `openspec/changes/flyway-schema-migration-setup/migration-troubleshooting-guide.md`
- **Docker 測試**: `openspec/changes/flyway-schema-migration-setup/docker-testing-guide.md`

## 開發指南

### 本地開發設定

#### 1. 後端開發

```bash
cd maplay
dotnet restore
dotnet build
dotnet run
```

**API 端點**: http://localhost:8080

#### 2. 前端開發

```bash
cd frontend  # 如有前端目錄
npm install
npm run dev
```

**前端端點**: http://localhost:3000 (如有)

### 專案結構

```
maplay/
├── db/migration/              # Flyway 遷移檔案
├── maplay/                   # 後端專案
│   ├── Controllers/          # API 控制器
│   ├── Data/                 # 資料存取層
│   │   ├── Entities/         # EF Core 實體
│   │   └── Repositories/     # Repository 模式
│   ├── Services/             # 業務邏輯層
│   └── Middleware/           # 中介層
├── compose.yaml              # Docker Compose 配置
├── flyway.conf              # Flyway 設定檔
├── .env.example             # 環境變數範本
└── DEVELOPER_GUIDE.md        # 開發者手冊
```

### EF Core 整合注意事項

**重要**: EF Core 僅用於查詢，禁止管理 schema！

```csharp
// ✅ 正確使用
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e => 
        {
            e.ToTable("users");  // 對應 Flyway 建立的表
            e.HasKey(x => x.Id);
        });
    }
}

// ❌ 禁止使用
// context.Database.Migrate();      // 嚴禁！schema 由 Flyway 管理
// context.Database.EnsureCreated(); // 嚴禁！
```

## API 文檔

### 互動式文檔

專案使用 **.NET 10 原生 OpenAPI** + **Scalar UI** 提供現代化的 API 文檔：

- 📖 **Scalar UI**: http://localhost:8080/scalar/v1
- 📄 **OpenAPI 規範**: http://localhost:8080/openapi/v1.json
- 🧪 **測試腳本**: `.\test-api.ps1`

### 文檔特色

- ✅ **自動生成**: 從程式碼 XML 註釋自動生成
- 🔍 **即時測試**: 在瀏覽器中直接測試 API
- 🔐 **JWT 認證**: 支援 Bearer Token 認證測試
- 📝 **完整範例**: 包含請求/回應範例
- 🎨 **現代介面**: Scalar UI 提供更好的使用體驗

### 主要端點

- `POST /api/auth/register` - 使用者註冊
- `POST /api/auth/login` - 使用者登入
- `GET /api/auth/me` - 獲取當前用戶資訊 (需認證)
- `GET /api/spots` - 景點列表 (分頁)
- `GET /api/spots/{id}` - 景點詳情
- `POST /api/spots` - 新增景點 (需認證)
- `GET /api/reviews` - 評價列表

### JWT 認證測試流程

1. **取得 JWT Token**:
   ```bash
   # 註冊���用戶
   curl -X POST http://localhost:8080/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!","displayName":"測試用戶"}'

   # 或登入取得 Token
   curl -X POST http://localhost:8080/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!"}'
   ```

2. **在 Swagger UI 中設定 Token**:
   - 點擊右上角「🔑 設定 Token」按鈕
   - 貼上您的 JWT Token (不包含 Bearer 前綴)
   - 重新載入頁面使設定生效

3. **測試需要認證的端點**:
   - 在 Swagger UI 中找到需要認證的端點 (標有🔐圖示)
   - 點擊「Try it out」執行測試
   - Token 會自動附加到請求頭中

### 認證機制

- **JWT Token**: Access token (15分鐘) + Refresh token (7天)
- **OAuth**: 支援 Google、LINE 登入
- **Bearer Token**: 請求頭格式 `Authorization: Bearer {token}`

📚 **詳細使用說明**: [OPENAPI_GUIDE.md](./OPENAPI_GUIDE.md)

## 部署

### Docker 部署

```bash
# 建置映像檔
docker compose build

# 生產環境部署
docker compose -f docker-compose.prod.yml up -d

# 查看狀態
docker compose ps
docker compose logs
```

### 環境變數

生產環境必須設定以下環境變數：

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=<生產資料庫連線>
Jwt__Secret=<安全的 JWT 密鑰>
OAuth__Google__ClientId=<Google OAuth Client ID>
OAuth__Line__ChannelId=<LINE Channel ID>
Storage__Endpoint=<物件儲存端點>
```

## 測試

### 執行測試

```bash
# 單元測試
dotnet test

# 整合測試
docker compose -f docker-compose.test.yml up -d
dotnet test --integration

# API 測試
curl http://localhost:8080/health
curl http://localhost:8080/api/spots?page=1&pageSize=10
```

## 常見問題

### Docker 問題

**Q: 容器無法啟動**
```bash
# 檢查 Docker Desktop 運行狀態
docker version

# 檢查容器日誌
docker compose logs <service_name>

# 重新建置
docker compose down -v && docker compose up -d
```

### 資料庫問題

**Q: 遷移執行失敗**
```bash
# 檢查遷移日誌
docker compose logs flyway

# 修復並重新執行
docker compose exec flyway flyway repair
docker compose restart flyway
```

**Q: 資料庫連線失敗**
```bash
# 檢查資料庫服務
docker compose ps db

# 測試連線
docker exec -it maplay-db-1 psql -U maplay -d maplay
```

## 貢獻指南

### 開發流程

1. Fork 專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 建立 Pull Request

### 程式碼規範

- **C#**: 遵循 .NET 命名約定
- **SQL**: 使用 snake_case 命名
- **JavaScript**: 遵循 ESLint 配置
- **Commit**: 使用 Conventional Commits 格式

## 授權

MIT License - 詳見 LICENSE 檔案

## 聯絡方式

- **專案 Issues**: GitHub Issues
- **文件**: DEVELOPER_GUIDE.md
- **技術支援**: 開發團隊

---

**最後更新**: 2026-06-29
**版本**: 1.0.0