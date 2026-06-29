# 遷移錯誤排查指南

## 概述

本指南提供 Flyway 遷移過程中常見錯誤的診斷與解決方法。每個錯誤包含症狀、原因分析、解決步驟與預防措施。

## 常見錯誤類型

### 1. 資料庫連線錯誤

#### 症狀
```
Unable to obtain connection from database (postgresql): 
Could not connect to PostgreSQL: Connection refused
```

#### 可能原因
1. PostgreSQL 服務未啟動
2. 連線資訊配置錯誤 (host, port, user, password)
3. 網路連線問題
4. Firewall 阻擋

#### 解決步驟
```bash
# 1. 檢查 PostgreSQL 服務狀態
docker compose ps db
docker compose logs db --tail=20

# 2. 本地測試連線
psql -h localhost -U maplay -d maplay -c "SELECT version();"

# 3. 檢查連線配置
cat flyway.conf | grep -E "(url|user|password)"

# 4. Docker 環境檢查
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "SELECT version();"
```

#### 解決方案
```bash
# 啟動 PostgreSQL 服務
docker compose up -d db

# 等待服務就緒
docker compose logs db --follow
# 等待看到 "database system is ready to accept connections"

# 更新連線配置
export FLYWAY_URL=jdbc:postgresql://localhost:5432/maplay
export FLYWAY_USER=maplay
export FLYWAY_PASSWORD=maplay
```

#### 預防措施
- 使用 Docker Compose 確保服務依賴順序
- 配置 healthcheck 確保資料庫就緒後才執行遷移
- 使用環境變數管理連線資訊

---

### 2. 語法錯誤

#### 症狀
```
ERROR: Syntax error in SQL statement
SQL:  CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    ...
);
```

#### 可能原因
1. SQL 語法錯誤
2. 拼字錯誤 (關鍵字、資料表名、欄位名)
3. 遺漏必要的符號 (括號、逗號、分號)
4. 不支援的 SQL 功能

#### 解決步驟
```bash
# 1. 檢查 Flyway 錯誤日誌
docker compose logs flyway | grep -A 10 "ERROR"

# 2. 手動測試 SQL 語法
psql -h localhost -U maplay -d maplay -f db/migration/V{version}__{description}.sql

# 3. 使用 SQL 編輯器驗證語法
# 許多 IDE (如 DataGrip, VS Code) 提供 PostgreSQL 語法檢查
```

#### 解決方案
```sql
-- 常見語法錯誤修正

-- ❌ 錯誤：遺漏逗號
CREATE TABLE users (
    id UUID PRIMARY KEY
    email VARCHAR(255) NOT NULL  -- 缺少逗號
);

-- ✅ 正確
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL
);

-- ❌ 錯誤：關鍵字拼寫錯誤
CREAT TABLE users (             -- CREATE 拼寫錯誤
    id UUID PRIMARY KEY
);

-- ❌ 錯誤：資料類型不存在
CREAT TABLE test (
    id STRING PRIMARY KEY        -- PostgreSQL 無 STRING 類型
);

-- ✅ 正確
CREATE TABLE test (
    id VARCHAR(255) PRIMARY KEY
);
```

#### 預防措施
- 使用支援 PostgreSQL 的 SQL 編輯器
- 在提交前測試 SQL 語法
- 遵循 PostgreSQL 官方文件語法
- 建立程式碼 review 流程

---

### 3. 依賴錯誤

#### 症狀
```
ERROR: Relation "users" does not exist
ERROR: foreign key constraint "fk_orders_customer" cannot be implemented
```

#### 可能原因
1. FK 引用的資料表不存在
2. 版本順序錯誤 (依賴的遷移未執行)
3. 資料表名稱拼寫錯誤

#### 解決步驟
```bash
# 1. 檢查遷移執行順序
flyway info

# 2. 確認目標資料表是否存在
psql -c "\dt" | grep users

# 3. 檢查遷移歷史
psql -c "SELECT version, description FROM flyway_schema_history ORDER BY installed_rank;"

# 4. 分析依賴關係
# 繪製資料��依賴圖確認順序
```

#### 解決方案
```sql
-- 問題：V3 建立的表引用 V4 的表 (順序錯誤)

-- V3__create_orders.sql (錯誤)
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    CONSTRAINT fk_orders_customer
        FOREIGN KEY (customer_id) REFERENCES customers (id)  -- customers 在 V4
);

-- 解決方案 1：調整版本順序
# 將 V3 重新命名為 V5 (在 V4 之後)

-- 解決方案 2：修改依賴關係
# 如果 customers 表非必需，可以先建立不包含 FK 的版本
```

#### 預防措施
- 執行遷移前檢查依賴關係
- 使用自動化依賴檢查工具
- 遵循「基礎表優先」原則
- 建立清晰的依賴關係圖

---

### 4. 資料冲突錯誤

#### 症狀
```
ERROR: duplicate key value violates unique constraint "uq_users_email"
ERROR: value too long for type character varying(20)
```

#### 可能原因
1. UNIQUE 約束冲突
2. 資料長度超過限制
3. CHECK 約束違反
4. 資料類型不匹配

#### 解決步驟
```bash
# 1. 檢查冲突資料
psql -c "SELECT email, COUNT(*) FROM users GROUP BY email HAVING COUNT(*) > 1;"

# 2. 檢查約束定義
psql -c "\d users" | grep -E "(Check constraints|Indexes)"

# 3. 查看相關資料
psql -c "SELECT * FROM users WHERE email = 'conflicting@example.com';"
```

#### 解決方案
```sql
-- 問題：重複 email 值違反唯一約束

-- 清理重複資料
WITH duplicates AS (
    SELECT email, ARRAY_AGG(id) AS ids
    FROM users
    WHERE email IS NOT NULL
    GROUP BY email
    HAVING COUNT(*) > 1
)
DELETE FROM users
WHERE id IN (
    SELECT unnest(ids)[2:array_length(ids, 1)]  -- 保留第一個，刪除其餘
    FROM duplicates
);

-- 問題：資料長度超過限制

-- 調整資料長度或修改欄位限制
UPDATE users SET role = 'user' WHERE LENGTH(role) > 20;

-- 或增加欄位長度
ALTER TABLE users ALTER COLUMN role TYPE VARCHAR(50);
```

#### 預防措施
- 在遷移前備份資料
- 測試遷移對現有資料的影響
- 使用事務包裹遷移，失敗可回滾
- 提供資料清理腳本

---

### 5. 權限錯誤

#### 症狀
```
ERROR: permission denied for table users
ERROR: must be owner of table users
```

#### 可能原因
1. 資料庫使用者權限不足
2. 資料表擁有者錯誤
3. Schema 權限配置問題

#### 解決步驟
```bash
# 1. 檢查當前使用者
psql -c "SELECT current_user;"

# 2. 檢查資料表擁有者
psql -c "SELECT tablename, tableowner FROM pg_tables WHERE tablename = 'users';"

# 3. 檢查使用權限
psql -c "\dp users"  # 顯示資料表權限
```

#### 解決方案
```sql
-- 授予必要權限
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO maplay;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO maplay;
GRANT USAGE ON SCHEMA public TO maplay;

-- 設定資料表擁有者
ALTER TABLE users OWNER TO maplay;

-- 設定 schema 擁有者
ALTER SCHEMA public OWNER TO maplay;
```

#### 預防措施
- 在 Docker Compose 中設定正確的使用者權限
- 使用專用的資料庫使用者 (不使用 postgres)
- 定期檢查權限配置
- 文件化權限需求

---

### 6. 版本冲突錯誤

#### 症狀
```
ERROR: Validation failed: Migration checksum mismatch for migration V8
ERROR: Detected resolved migration not applied to database: V9
```

#### 可能原因
1. 遷移檔案內容被修改 (checksum 變更)
2. 手動修改資料庫 schema
3. 不同環境遷移狀態不一致

#### 解決步驟
```bash
# 1. 檢查遷移狀態
flyway info

# 2. 檢查 checksum
flyway validate

# 3. 查看資料庫實際狀態
psql -c "SELECT version, checksum, description FROM flyway_schema_history ORDER BY installed_rank;"

# 4. 比對本地遷移檔案
md5sum db/migration/V*.sql
```

#### 解決方案
```bash
# 方案 1: 修復 checksum (如果檔案變更是合法的)
flyway repair

# 方案 2: 回滾到先前的正確狀態
# 恢復修改前的遷移檔案
flyway repair

# 方案 3: 建立新的遷移覆蓋變更 (推薦)
# 不修改已執行的遷移，建立新遷移修正問題
```

#### 預防措施
- 禁止修改已執行的遷移檔案
- 使用版本控制追蹤所有變更
- 在不同環境保持遷移同步
- 定期備份資���庫

---

### 7. 環境差異錯誤

#### 症狀
```
ERROR: type "geometry" does not exist
ERROR: function gen_random_uuid() does not exist
```

#### 可能原因
1. 缺少必要的 extensions
2. PostgreSQL 版本不兼容
3. 不同環境的設置差異

#### 解決步驟
```bash
# 1. 檢查已安裝的 extensions
psql -c "SELECT * FROM pg_extension;"

# 2. 檢查 PostgreSQL 版本
psql -c "SELECT version();"

# 3. 比較不同環境的設置
docker exec -it maplay-db-1 psql -c "SELECT * FROM pg_extension;"
```

#### 解決方案
```sql
-- 安裝缺失的 extensions
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 驗證安裝
SELECT * FROM pg_extension WHERE extname IN ('postgis', 'pgcrypto');
```

#### 預防措施
- 在基礎遷移 (V1) 中建立所有必要的 extensions
- 使用統一的 PostgreSQL 版本
- 文件化所有必要的 extensions
- 在 CI/CD 中驗證環境一致性

---

### 8. 效能問題

#### 症狀
```
Migration taking too long...
Timeout waiting for database response...
```

#### 可能原因
1. 大量資料遷移
2. 缺少索���導致慢查詢
3. 鎖表競爭
4. 資源限制

#### 解決步驟
```bash
# 1. 監控遷移進度
docker compose logs flyway --follow

# 2. 檢查資料庫活動
psql -c "SELECT pid, query, state FROM pg_stat_activity WHERE state = 'active';"

# 3. 查看鎖表狀況
psql -c "SELECT * FROM pg_locks WHERE NOT granted;"

# 4. 檢查資源使用
docker stats maplay-db-1
```

#### 解決方案
```sql
-- 優化大量資料處理
-- 分批處理代替單次大批量
BEGIN;
INSERT INTO new_table SELECT * FROM old_table LIMIT 1000;
COMMIT;

-- 重複執行直到完成

-- 暫時停用索引加快資料匯入
DROP INDEX CONCURRENTLY IF EXISTS ix_users_email;
-- 執行資料匯入
CREATE INDEX CONCURRENTLY ix_users_email ON users (email);

-- 使用 EXPLAIN 分析查詢效能
EXPLAIN ANALYZE SELECT * FROM large_table WHERE condition;
```

#### 預防措施
- 在測試環境評估遷移執行時間
- 對大型遷移設定合理的超時時間
- 使用批次處理大量資料
- 監控資源使用情況

---

## 診斷工具與技巧

### 基礎診斷指令

```bash
# 檢查 Flyway 狀態
flyway info
flyway validate
flyway status

# 查看遷移日誌
docker compose logs flyway --tail=50

# 檢查資料庫狀態
psql -c "\dt"                    # 列出資料表
psql -c "\d table_name"          # 檢查資料表結構
psql -c "SELECT * FROM flyway_schema_history;"  # 查看遷移歷史

# 測試 SQL 語法
psql -f db/migration/V{version}__{description}.sql
```

### 進階診斷技巧

#### 1. 問題重現
```bash
# 使用空資料庫測試
docker volume rm maplay_pgdata
docker compose up -d

# 單獨測試有問題的遷移
flyway migrate -target=V8
```

#### 2. 對比環境
```bash
# 比較開發與生產環境的遷移狀態
# 開發環境
flyway info > dev_migration_state.txt

# 生產環境
flyway info > prod_migration_state.txt

# 比較差異
diff dev_migration_state.txt prod_migration_state.txt
```

#### 3. 資料一致性檢查
```sql
-- 檢查 FK 約束是否被違反
SELECT
    tc.table_name,
    tc.constraint_name,
    ccu.table_name AS foreign_table
FROM information_schema.table_constraints AS tc
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND NOT EXISTS (
        SELECT 1 FROM pg_constraint pc
        WHERE pc.conname = tc.constraint_name
        AND pc.convalidated = true
    );
```

## 緊急修復程序

### 重大錯誤應對流程

1. **停止執行**
   ```bash
   docker compose down
   # 或
   flyway stop
   ```

2. **評估影響**
   ```bash
   # 檢查有多少遷移已執行
   flyway info
   ```

3. **備份現狀**
   ```bash
   # 備份資料庫
   pg_dump -h localhost -U maplay -d maplay > emergency_backup.sql
   ```

4. **決定修復策略**
   - 回滾到先前狀態 (如果可能)
   - 修復問題遷移並重新執行
   - 建立修復遷移

5. **修復與驗證**
   ```bash
   # 執行修復
   flyway repair
   flyway migrate
   
   # 驗證結果
   flyway validate
   psql -c "\dt"
   ```

## 預防措施總結

### 開發階段
- [ ] 使用 SQL 編輯器檢查語法
- [ ] 在測試環境充分驗證
- [ ] 檢查依賴關係順���
- [ ] 評估對現有資料的影響

### 部署階段
- [ ] 備份生產資料庫
- [ ] 使用事務保證原子性
- [ ] 監控遷移執行過程
- [ ] 準備回滾計畫

### ��護階段
- [ ] 定期檢查���移狀態一致性
- [ ] 文件化所有變更
- [ ] 維護版本同步
- [ ] 建立監控警報

## 相關資源

- [Flyway 官方文件 - Troubleshooting](https://flywaydb.org/documentation/troubleshooting/)
- [PostgreSQL 錯誤碼大全](https://www.postgresql.org/docs/current/errcodes-appendix.html)
- 專案依賴分析: `.dependency-analysis.md`
- Docker 測試指南: `docker-testing-guide.md`