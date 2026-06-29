# 開發者手冊

## 專案概述

親子資源地圖系統 (Family Resource Map) - 整合公園、親子餐廳、哺乳室廁所、活動、醫療等親子資源地圖應用。

## 技術架構

### 前端
- **框架**: Vue 3 + Vite + Pinia + Vue Router
- **地圖**: Leaflet.js (OpenStreetMap 底圖)
- **部署**: 靜態檔案服務

### 後端
- **框架**: ASP.NET Core 8 Web API
- **ORM**: EF Core + Npgsql + NetTopologySuite (僅查詢用)
- **認證**: JWT + OAuth (Google/Line)
- **部署**: Docker 容器化

### 資料庫
- **DBMS**: PostgreSQL 16 + PostGIS
- **遷移**: Flyway 版本化 SQL (schema 唯一來源)
- **空間資料**: WGS84 / SRID 4326

### 儲存
- **物件儲存**: S3 相容 (MinIO 本機開發)
- **圖片管理**: URL + object_key 儲存

## 開發環境設定

### 前置要求
```bash
# .NET 8 SDK
dotnet --version

# Node.js 18+
node --version

# Docker Desktop
docker --version
```

### 本地開發設定

#### 1. 複製環境變數檔案
```bash
cp .env.example .env
# 根據本地環境調整設定值
```

#### 2. 啟動資料庫服務
```bash
# 啟動 PostgreSQL + PostGIS
docker compose up -d db minio

# 等待服務就緒
docker compose logs db --follow
```

#### 3. 執行資料庫遷移
```bash
# Flyway 自動執行遷移
docker compose up -d flyway

# 檢查遷移狀態
docker compose logs flyway
```

#### 4. 啟動後端 API
```bash
# 方式 1: 使用 Docker
docker compose up -d maplay

# 方式 2: 本地運行
cd maplay
dotnet restore
dotnet build
dotnet run
```

#### 5. 啟動前端 (如有需要)
```bash
cd frontend  # 假設前端目錄
npm install
npm run dev
```

### IDE 設定建議

#### Visual Studio / Rider
- 設定啟動專案: `maplay`
- 設定環境變數: `ASPNETCORE_ENVIRONMENT=Development`
- 熱重載: 啟用 (僅開發時)

#### VS Code (前端)
- 安裝 Volar extension
- 設定格式: Prettier + ESLint
- 型別檢查: 嚴格模式

## 資料庫遷移 (Flyway)

### 基本概念

本專案使用 **Flyway** 作為資料庫 schema 版本控制工具：
- **Schema 擁有權**: 歸 Flyway 管理
- **EF Core 角色**: 僅用於查詢與物件對應
- **遷移檔案**: 版本化 SQL，存於 `db/migration/`

### 遷移檔案結構

```
db/migration/
├── V1__enable_postgis.sql
├── V2__create_users.sql
├── V3__create_refresh_tokens.sql
├── V4__create_spots.sql
├── V5__create_spot_images.sql
├── V6__create_reviews.sql
└── V7__create_import_history.sql
```

### 新增遷移檔案

#### 1. 命名規則
```
V{version}__{description}.sql
```
- `version`: 遞增整數 (V8, V9, V10...)
- `description`: 小寫字母與底線 (create_user_profiles)

#### 2. 基本結構
```sql
-- V8: user_profiles 表（FK → users）
-- 用途：儲存使用者詳細資料

CREATE TABLE user_profiles (
    id         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID        NOT NULL,
    bio        TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_user_profiles_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_profiles_user_id ON user_profiles (user_id);
```

#### 3. 依賴檢查
- 確保 FK 引用的資料表在先前版本建立
- 檢查版本順序正確
- 驗證無循環依賴

#### 4. 測試遷移
```bash
# Docker 環境測試
docker compose down -v              # 清理舊資料
docker compose up -d db flyway      # 重新執行遷移
docker compose logs flyway          # 檢查結果

# 或使用 Flyway CLI
flyway migrate
flyway validate
```

### 常用 Flyway 指令

```bash
# 查看遷移狀態
flyway info

# 執行待處理遷移
flyway migrate

# 驗證遷移一致性
flyway validate

# 修復 checksum (當檔案被合法修改時)
flyway repair
```

### EF Core 整合

**重要**: EF Core 僅用於查詢，禁止使用 Migrations！

```csharp
// ✅ 正確使用方式
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");              // 對應 Flyway 建立的表
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        });
    }
}

// ❌ 禁止使用
// context.Database.Migrate();      // 嚴禁！
// context.Database.EnsureCreated(); // 嚴禁！
```

### 相關文件
- 詳細工作流程: `openspec/changes/flyway-schema-migration-setup/migration-workflow-guide.md`
- 命名規則: `openspec/changes/flyway-schema-migration-setup/migration-naming-dependency-rules.md`
- 錯誤排查: `openspec/changes/flyway-schema-migration-setup/migration-troubleshooting-guide.md`

## API 開發指南

### 專案結構
```
maplay/
├── Controllers/        # API 控制器
├── Data/
│   ├── Entities/      # EF Core 實體
│   ├── Repositories/  # 資料存取層
│   └── AppDbContext.cs
├── Services/          # 業務邏輯層
├── Middleware/        # 中介層
└── Models/           # DTO 與 Request/Response
```

### API 規範

#### 路由設計
```csharp
// RESTful 設計原則
[HttpGet("api/spots")]
[HttpGet("api/spots/{id}")]
[HttpPost("api/spots")]
[HttpPut("api/spots/{id}")]
[HttpDelete("api/spots/{id}")]
```

#### 分頁約定
```csharp
// 強制分頁，防止大量資料查詢
[HttpGet("api/spots")]
public async Task<IActionResult> GetSpots(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    if (pageSize > 100) pageSize = 100; // 上限限制
    // ...
}
```

#### 回應格式
```csharp
// 成功回應
return Ok(new { data, pagination });

// 錯誤回應
return BadRequest(new { 
    errorCode = "INVALID_INPUT",
    message = "Validation failed" 
});
```

#### 權限控制
```csharp
// 使用授權屬性
[Authorize]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // ...
}
```

### 資料存取模式

#### Repository 模式
```csharp
public interface ISpotRepository
{
    Task<Spot?> GetByIdAsync(Guid id);
    Task<PagedResult<Spot>> GetPagedAsync(int page, int pageSize);
    Task<Spot> CreateAsync(Spot spot);
}

public class SpotRepository : ISpotRepository
{
    private readonly AppDbContext _context;
    
    public async Task<Spot?> GetByIdAsync(Guid id)
        => await _context.Spots.FindAsync(id);
}
```

#### 查詢最佳化
```csharp
// 使用 AsNoTracking 提升唯讀查詢效能
var spots = await _context.Spots
    .AsNoTracking()
    .Where(s => s.Status == "approved")
    .ToListAsync();

// 必要時使用 Include 載入關聯資料
var spot = await _context.Spots
    .Include(s => s.Images)
    .Include(s => s.Reviews)
    .FirstOrDefaultAsync(s => s.Id == id);
```

## 測試指南

### 單元測試
```bash
cd maplay.Tests
dotnet test
```

### 整合測試
```bash
# 使用測試資料庫
docker compose -f docker-compose.test.yml up -d
dotnet test --integration
```

### API 測試
```bash
# 健康檢查
curl http://localhost:8080/health

# 景點列表
curl http://localhost:8080/api/spots?page=1&pageSize=10

# 認證測試
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'
```

## 部署流程

### Docker 部署
```bash
# 建置映像檔
docker compose build

# 完整部署
docker compose up -d

# 檢查狀態
docker compose ps
docker compose logs
```

### 環境變數設定
```bash
# 生產環境必須設定的變數
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=<生產資料庫連線>
Jwt__Secret=<安全的 JWT 密鑰>
OAuth__Google__ClientId=<Google OAuth Client ID>
OAuth__Line__ChannelId=<LINE Channel ID>
Storage__Endpoint=<物件儲存端點>
```

## 常見問題

### 開發問題

**Q: Docker 容器無法啟動**
```bash
# 檢查 Docker Desktop 運行狀態
docker version

# 檢查容器日誌
docker compose logs <service_name>

# 重新建置
docker compose down -v
docker compose up -d
```

**Q: 資料庫連線失敗**
```bash
# 檢查資料庫服務
docker compose ps db

# 測試連線
docker exec -it maplay-db-1 psql -U maplay -d maplay

# 檢查連線字串
echo $ConnectionStrings__Default
```

**Q: 遷移執行失敗**
```bash
# 檢查遷移日誌
docker compose logs flyway

# 驗證遷移狀態
docker compose exec flyway flyway info

# 查看遷移錯誤排查指南
cat openspec/changes/flyway-schema-migration-setup/migration-troubleshooting-guide.md
```

### 效能問題

**Q: API 回應慢**
```bash
# 檢查資料庫查詢效能
docker exec -it maplay-db-1 psql -U maplay -d maplay
EXPLAIN ANALYZE <your_query>;

# 檢查應用程式日誌
docker compose logs maplay --tail=50
```

**Q: 資料庫效能問題**
```bash
# 檢查慢查詢
SELECT query, mean_exec_time FROM pg_stat_statements ORDER BY mean_exec_time DESC;

# 檢查索引使用情況
SELECT schemaname, tablename, indexname, idx_scan FROM pg_stat_user_indexes;
```

## 開發規範

### 程式碼風格

#### C# 約定
- 使用 PascalCase 命名類別、方法、屬性
- 使用 camelCase 命名方法參數、區域變數
- 私有欄位使用 _camelCase

#### SQL 約定
- 資料表名稱使用 snake_case (users, spot_images)
- 欄位名稱使用 snake_case (created_at, user_id)
- 索引名稱: ix_{table}_{column} (ix_users_email)
- FK 約束: fk_{table}_{ref_table} (fk_orders_user)
- 唯一約束: uq_{table}_{column} (uq_users_email)

### Git 工作流程

#### 分支策略
- `master`: 主分支，生產環境
- `dev`: 開發分支，整合測試
- `feature/*`: 功能分支

#### Commit 訊息規範
```
Add user authentication system

- Implement JWT token generation and validation
- Add OAuth providers (Google, LINE)
- Update database schema with user-related tables
- Create authentication controller and services

Refs: #123
```

### 安全性注意事項

#### 認證授權
- 所有敏感端點使用 `[Authorize]`
- 管理員功能使用 `[Authorize(Roles = "Admin")]`
- JWT Token 有效期: access token 15 分鐘, refresh token 7 天

#### 資料驗證
- 所有使用者輸入必須驗證
- SQL 查詢使用參數化 (防止 SQL injection)
- 檔案上傳限制大小與類型

#### 敏感資料
- 密碼使用 BCrypt 雜湊
- API Key 不提交到程式庫
- 生產環境使用 HTTPS

## 相關資源

### 專案文件
- [專案架構設計](./DESIGN.md) (如有)
- [API 文件](./API.md) (如有)
- [部署指南](./DEPLOYMENT.md) (如有)

### 技術文件
- [ASP.NET Core 文件](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Flyway 文件](https://flywaydb.org/documentation/)
- [PostgreSQL 文件](https://www.postgresql.org/docs/)
- [PostGIS 文件](https://postgis.net/documentation/)

### 工具資源
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL Client](https://www.pgadmin.org/)
- [API 測試工具](https://www.postman.com/)

---

**最後更新**: 2026-06-29
**維護者**: 開發團隊