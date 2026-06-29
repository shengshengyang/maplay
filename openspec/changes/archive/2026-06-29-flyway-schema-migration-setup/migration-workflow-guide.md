# 新增遷移檔案標準流程

## 概述

本專案使用 Flyway 進行資料庫 schema 版本控制，所有 schema 變更必須通過版本化 SQL 遷移檔案管理。本文件說明新增遷移檔案的標準流程。

## 前置準備

### 環境確認
- [ ] PostgreSQL 16+ 已安裝並可連線
- [ ] Flyway CLI 已安裝 (或使用 Docker Compose)
- [ ] 已了解現有 schema 結構與依賴關係

### 檢查現有遷移
```bash
# 查看已執行的遷移
flyway info

# 或使用 Docker
docker compose exec flyway flyway info
```

## 新增遷移檔案流程

### 1. 分析變更需求

#### 確認變更類型
- **新資料表**: 確定 FK 依賴關係
- **新增欄位**: 確定是否需要預設值、約束
- **修改欄位**: 評估對現有資料的影響
- **建立索引**: 考慮查詢效能與寫入效能平衡
- **新增約束**: 確認不會違反現有資料

#### 檢查依賴關係
```sql
-- 範例：如果要建立新表 user_profiles 依賴 users 表
-- 1. 確認 users 表已存在 (V2)
-- 2. 新檔案版本號應 > V2
-- 3. FK constraint 必須 REFERENCES users(id)
```

### 2. 建立遷移檔案

#### 檔案命名規則
```
V{version}__{description}.sql
```

**命名慣例:**
- `version`: 遞增整數 (V8, V9, V10...)
- `description`: 小寫字母與底線 (create_user_profiles, add_status_to_reviews)
- **禁止**: 使用時間戳記、重複版本號、跳號

#### 檔案位置
```
maplay/
└── db/
    └── migration/
        ├── V1__enable_postgis.sql
        ├── V2__create_users.sql
        ├── ...
        └── V8__create_user_profiles.sql  # 新檔案
```

### 3. 撰寫遷移 SQL

#### 基本結構
```sql
-- V8: user_profiles 表（FK → users）
-- 用途：儲存使用者詳細資料，包含偏好設定與個人簡介

CREATE TABLE user_profiles (
    id         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID        NOT NULL,
    bio        TEXT,
    preferences JSONB     DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_user_profiles_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,

    CONSTRAINT chk_user_profiles_preferences
        CHECK (preferences IS NULL OR jsonb_typeof(preferences) = 'object')
);

-- 索引建立
CREATE INDEX ix_user_profiles_user_id ON user_profiles (user_id);
CREATE INDEX ix_user_profiles_preferences ON user_profiles USING GIN (preferences);
```

#### SQL ���寫最佳實踐

##### 1. 資料表定義
- **主鍵**: 統一使用 `UUID PRIMARY KEY DEFAULT gen_random_uuid()`
- **時間戳記**: `created_at TIMESTAMPTZ NOT NULL DEFAULT now()`
- **更新時間**: `updated_at TIMESTAMPTZ NOT NULL DEFAULT now()`
- **欄位命名**: 使用 snake_case (user_id, created_at)

##### 2. Foreign Key 設計
- **CASCADE DELETE**: 強關聯 (orders → order_items)
- **SET NULL**: 弱關聯 (spots.submitted_by)
- **RESTRICT**: 防��誤刪 (重要關聯)

##### 3. 索引策略
```sql
-- FK 欄位預設建立索引
CREATE INDEX ix_{table}_{fk_column} ON {table} ({fk_column});

-- 查詢欄位建立索引
CREATE INDEX ix_{table}_{column} ON {table} ({column});

-- 唯一約束
CREATE UNIQUE INDEX uq_{table}_{column} ON {table} ({column});

-- 部分唯一索引
CREATE UNIQUE INDEX uq_{table}_{column}
    ON {table} ({column})
    WHERE {column} IS NOT NULL;
```

##### 4. 約束定義
```sql
-- CHECK 約束
CONSTRAINT chk_{table}_{rule} CHECK ({column} IN ('value1', 'value2'))

-- NOT NULL 約束
{column} {type} NOT NULL

-- DEFAULT 值
{column} {type} NOT NULL DEFAULT {value}
```

##### 5. 特殊類型處理
```sql
-- PostGIS geometry
location geometry(Point, 4326) NOT NULL

-- 陣列類型
tags TEXT[] DEFAULT '{}'

-- JSONB
metadata JSONB DEFAULT '{}'

-- TEXT 類型 (無長度限制)
description TEXT
```

### 4. 測試遷移

#### 本地測試
```bash
# 1. 備份現有資料庫 (重要!)
pg_dump -h localhost -U maplay -d maplay > backup_$(date +%Y%m%d).sql

# 2. 測試遷移
flyway migrate

# 3. 驗證結果
flyway info

# 4. 檢查資料表
psql -h localhost -U maplay -d maplay -c "\dt"
psql -h localhost -U maplay -d maplay -c "\d user_profiles"
```

#### Docker 測試
```bash
# 1. 清理舊環境 (測試用)
docker compose down -v

# 2. 重新執行所有遷移
docker compose up -d

# 3. 檢查 Flyway 日誌
docker compose logs flyway

# 4. 驗證資料表
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "\d user_profiles"
```

### 5. 驗證遷移結果

#### 結構驗證
```sql
-- 檢查資料表是否建立
\d user_profiles

-- 檢查索引是否建立
\di user_profiles*

-- 檢查約束是否建立
SELECT constraint_name, constraint_type
FROM information_schema.table_constraints
WHERE table_name = 'user_profiles';

-- 檢查 FK 關係
SELECT
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'user_profiles';
```

#### 功能測試
```sql
-- 測試資料插入
INSERT INTO user_profiles (user_id, bio, preferences)
VALUES ('123e4567-e89b-12d3-a456-426614174000', 'Test bio', '{"theme": "dark"}'::jsonb);

-- 測試 FK 約束
-- INSERT INTO user_profiles (user_id, bio) VALUES ('invalid-uuid', 'Test');
-- 應該失敗並報錯

-- 測試 CHECK 約束
-- INSERT INTO user_profiles (user_id, preferences) VALUES ('123e4567-e89b-12d3-a456-426614174000', 'not-json');
-- 應該失敗並報錯
```

### 6. 程式碼同步更新

#### 更新 EF Core 實體
```csharp
// maplay/Data/Entities/UserProfile.cs
public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
```

#### 更新 DbContext
```csharp
// maplay/Data/AppDbContext.cs
public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

protected override void OnModelCreating(ModelBuilder b)
{
    // ... existing mappings ...

    b.Entity<UserProfile>(e =>
    {
        e.ToTable("user_profiles");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.UserId).IsRequired();
        e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    });
}
```

#### 新增 Repository/Service (如需要)
```csharp
// maplay/Data/Repositories/IUserProfileRepository.cs
public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
}
```

### 7. 提交變更

#### Git Commit 訊息格式
```
Add user_profiles table with preferences

- V8__create_user_profiles.sql
- UserProfile entity with EF Core mapping
- UserProfileRepository for data access
- Update related dependencies

Dependencies: users table (V2)
Tested: Docker Compose full migration
```

#### 提交檔案清單
```bash
git add db/migration/V8__create_user_profiles.sql
git add maplay/Data/Entities/UserProfile.cs
git add maplay/Data/AppDbContext.cs  # if modified
git add maplay/Data/Repositories/IUserProfileRepository.cs
git commit -m "Add user_profiles table with preferences"
```

## 常見範例

### 新增欄位至現有表
```sql
-- V9: add_profile_image_to_users.sql
ALTER TABLE users ADD COLUMN profile_image_url TEXT;
CREATE INDEX ix_users_profile_image_url ON users (profile_image_url) WHERE profile_image_url IS NOT NULL;
```

### 建立索引
```sql
-- V10: add_spots_performance_indexes.sql
CREATE INDEX ix_spots_category_status ON spots (category, status) WHERE deleted_at IS NULL;
CREATE INDEX ix_spots_location_filter ON spots USING GIST (location) WHERE status = 'approved';
```

### 新增枚舉約束
```sql
-- V11: add_spot_type_constraint.sql
ALTER TABLE spots ADD CONSTRAINT chk_spots_spot_type_enhanced
CHECK (spot_type IN ('permanent', 'temporary', 'seasonal'));
```

## 注意事項

### ⚠️ 禁止事項
- **禁止**: 直接修改 production 資料庫
- **禁止**: 手動執行 DDL 而不建立遷移檔案
- **��止**: 修改已執行的遷移檔案 (建立新遷移覆蓋)
- **禁止**: 跳號版本 (V7 → V9)

### ⚠️ 風險控制
- **破壞性變更**: 必須評估對現有資料的影響
- **大量資料**: 考慮批次處理與執行時間
- **FK 新增**: 確認現有資料符合約束
- **索引新增**: 評估對寫入效能的影響

### ✅ 最佳實踐
- **可逆操作**: 考慮提供 rollback 腳本 (雖專案主要使用前進遷移)
- **註解完整**: 在檔案開頭說明用途與依賴
- **測試優先**: 在非 production 環境充分測試
- **文件同步**: 更新相關 API 文件與開發文件

## 相關資源

- [Flyway 官方文件](https://flywaydb.org/documentation/)
- [PostgreSQL 16 文件](https://www.postgresql.org/docs/16/)
- [PostGIS 手冊](https://postgis.net/documentation/)
- 專案內依賴分析報告: `.dependency-analysis.md`
- EF Core 對應驗證: `ef-core-flyway-mapping-validation.md`