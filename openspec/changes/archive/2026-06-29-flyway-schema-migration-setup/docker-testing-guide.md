# Docker Compose 測試指南

## 測試前準備

### 啟動 Docker Desktop
確保 Docker Desktop 已啟動並正常運行：
```bash
docker version
```

### 檢查現有設定
```bash
# 驗證 Docker Compose 配置語法
docker compose config

# 檢查是否有舊的容器/磁碟區需要清理
docker ps -a
docker volume ls
```

## 完整啟動測試

### 1. 清理舊環境 (可選)
```bash
# 停止並移除所有容器
docker compose down

# 移除舊的資料庫資料 (會清除所有資料!)
docker volume rm maplay_pgdata
```

### 2. 完整啟動流程
```bash
# 啟動所有服務
docker compose up -d

# 查看服務狀態
docker compose ps

# 查看特定服務日誌
docker compose logs flyway
docker compose logs db
docker compose logs maplay
```

### 3. 驗證遷移執行

#### 檢查 Flyway 執行結果
```bash
# Flyway 應該顯示成功執行所有遷移 (V1-V7)
docker compose logs flyway | grep -i "successfully"
```

#### 直接連線資料庫驗證
```bash
# 進入 PostgreSQL 容器
docker exec -it maplay-db-1 psql -U maplay -d maplay

# 在 psql 中執行驗證查詢
\dt                           # 列出所有資料表
\d users                      # 檢查 users 表結構
\d spots                      # 檢查 spots 表結構
SELECT * FROM flyway_schema_history;  # 檢查遷移歷史
\q                            # 退出
```

### 4. 驗證服務依賴順序
```bash
# 檢查服務啟動順序和依賴關係
docker compose ps

# 預期結果:
# 1. db (PostgreSQL) -> healthy
# 2. flyway -> completed successfully  
# 3. minio -> healthy
# 4. maplay -> running
```

### 5. 測試 Web API 功能
```bash
# 測試 API 健康檢查 (如果有的話)
curl http://localhost:8080/health

# 測試景點列表 API (預期回傳空陣列，因為無資料)
curl http://localhost:8080/api/spots
```

## 重啟驗證測試

### 測試遷移不被重複執行
```bash
# 停止服務
docker compose down

# 重新啟動 (模擬容器重啟)
docker compose up -d

# 檢查 Flyway 日誌 - 應該顯示 "Schema is up to date"
docker compose logs flyway | tail -20

# 驗證資料庫狀態未被重置
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "SELECT COUNT(*) FROM flyway_schema_history;"
# 應該顯示 7 筆記錄 (V1-V7)，不會新增
```

## 問題排查

### Flyway 執行失敗
```bash
# 查看詳細錯誤訊息
docker compose logs flyway --tail=50

# 進入 Flyway 容器手動執行
docker exec -it maplay-flyway-1 sh
flyway -url=jdbc:postgresql://db:5432/maplay -user=maplay -password=maplay info
```

### 資料庫連線問題
```bash
# 檢查 PostgreSQL 健康狀態
docker compose ps db
docker compose logs db --tail=20

# 手動測試連線
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "SELECT version();"
```

### 清��並重新開始
```bash
# 完���清理環境
docker compose down -v    # 移除容器和 volumes
docker compose up -d      # 重新建置並啟動
```

## 驗證清單

完成以下項目表示 Docker 整合成功：

- [ ] Docker Compose 配置語法正確 (`docker compose config`)
- [ ] PostgreSQL 服務啟動成功並通過 healthcheck
- [ ] Flyway 成功執行所有遷移 (V1-V7)
- [ ] `flyway_schema_history` 表包含 7 筆記錄
- [ ] 所有資料表正確建立 (users, spots, reviews, etc.)
- [ ] Web API 服務啟動成功
- [ ] 容器重啟時遷移不被重複執行
- [ ] MinIO 服務正常運行並建立 bucket