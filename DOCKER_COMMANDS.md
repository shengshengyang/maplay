# Docker 常用指令

## 快速啟動

### 完整版本（本地開發）
```powershell
# 一次啟動所有服務
docker-compose -f compose.yaml up -d

# 重建並啟動
docker-compose -f compose.yaml up -d --build
```

### 分離部署（推薦）
```powershell
# 啟動基礎設施（PostgreSQL + MinIO）
docker-compose -f compose-infrastructure.yaml up -d

# 啟動應用程式
docker-compose -f compose-app.yaml up -d --build
```

### Windows 修復版本（相容性問題時使用）
```powershell
# 清理並啟動修復版本
.\start-windows-fixed.bat

# 或手動執行
docker-compose -f compose-fix-windows.yaml up -d
```

## 服務管理

### 查看服務狀態
```powershell
# 查看所有服務狀態
docker-compose ps

# 查看特定 compose 檔案的服務
docker-compose -f compose-infrastructure.yaml ps
docker-compose -f compose-app.yaml ps
```

### 查看日誌
```powershell
# 查看所有服務日誌
docker-compose logs

# 查看特定服務日誌
docker-compose logs maplay
docker-compose logs db
docker-compose logs flyway

# 即時跟蹤日誌
docker-compose logs -f maplay

# 查看最後 100 行
docker-compose logs --tail=100 maplay
```

### 重啟服務
```powershell
# 重啟所有服務
docker-compose restart

# 重啟特定服務
docker-compose restart maplay
docker-compose restart db

# 重建並啟動應用程式（更新程式碼後）
docker-compose -f compose-app.yaml up -d --build maplay
```

## 停止與清理

### 停止服務
```powershell
# 停止所有服務（保留資料）
docker-compose down

# 停止特定 compose 檔案的服務
docker-compose -f compose-app.yaml down
docker-compose -f compose-infrastructure.yaml down

# 停止並移除容器
docker-compose -f compose.yaml down
```

### 清理資料
```powershell
# 清理所有未使用的 volumes（會刪除資料庫資料！）
docker volume prune

# 清理所有未使用的映像檔
docker image prune

# 清理所有未使用的容器、網路、映像檔
docker system prune

# 清理所有（包括未使用的映像檔）
docker system prune -a

# 停止並清理所有資料（危險！）
docker-compose down -v
```

## 進入容器

### 進入容器 Shell
```powershell
# 進入應用程式容器
docker exec -it maplay-maplay-1 /bin/bash

# 進入 PostgreSQL 容器
docker exec -it maplay-db-1 psql -U maplay -d maplay

# 進入 MinIO 容器
docker exec -it maplay-minio-1 /bin/sh
```

### 在容器中執行指令
```powershell
# 在 PostgreSQL 中執行 SQL
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "SELECT version();"

# 查看容器中的環境變數
docker exec maplay-maplay-1 env
```

## 資料庫操作

### 連接資料庫
```powershell
# 使用 psql 連接
docker exec -it maplay-db-1 psql -U maplay -d maplay

# 從本機連接（需要工具或透過端口轉發）
docker run -it --rm postgres:16 psql -h localhost -U maplay -d maplay
```

### 資料庫備份與還原
```powershell
# 備份資料庫
docker exec maplay-db-1 pg_dump -U maplay maplay > backup.sql

# 還原資料庫
docker exec -i maplay-db-1 psql -U maplay maplay < backup.sql

# 備份整個 PostgreSQL 資料目錄
docker cp maplay-db-1:/var/lib/postgresql/data ./pg_backup
```

### 查看資料庫大小
```powershell
# 透過 psql 查看資料庫大小
docker exec -it maplay-db-1 psql -U maplay -d maplay -c "
SELECT 
    pg_database.datname,
    pg_size_pretty(pg_database_size(pg_database.datname)) AS size 
FROM pg_database 
ORDER BY pg_database_size(pg_database.datname) DESC;
"
```

## 網路與除錯

### 查看容器網路
```powershell
# 查看所有網路
docker network ls

# 查看特定網路詳情
docker network inspect maplay_maplay-network

# 測試容器間連接
docker exec maplay-maplay-1 ping db
docker exec maplay-maplay-1 curl http://minio:9000
```

### 查看容器資源使用
```powershell
# 查看所有容器資源使用
docker stats

# 查看特定容器
docker stats maplay-db-1 maplay-maplay-1

# 查看容器詳細資訊
docker inspect maplay-db-1
```

## 故障排除

### 檢查容器健康狀態
```powershell
# 查看容器健康檢查狀態
docker inspect --format='{{.State.Health.Status}}' maplay-db-1

# 查看容器事件
docker events --since '1h'
```

### 重新初始化資料庫
```powershell
# 停止服務
docker-compose -f compose.yaml down

# 清理 volumes
docker volume rm maplay_pgdata maplay_miniodata

# 重新啟動（會重新初始化）
docker-compose -f compose.yaml up -d
```

### 檢查 Flyway 遷移狀態
```powershell
# 查看 Flyway 遷移歷史
docker exec maplay-flyway-1 flyway info

# 手動執行遷移
docker exec maplay-flyway-1 flyway migrate

# 驗證遷移狀態
docker exec maplay-flyway-1 flyway validate
```

## 開發工作流程

### 更新應用程式
```powershell
# 1. 修改程式碼後
# 2. 重建並重啟應用程式
docker-compose -f compose-app.yaml up -d --build maplay

# 3. 查看日誌確認啟動成功
docker-compose -f compose-app.yaml logs -f maplay
```

### 新增資料庫遷移
```powershell
# 1. 在 db/migration 目錄新增 SQL 檔案
# 2. 重啟 flyway 服務執行遷移
docker-compose restart flyway

# 或手動執行
docker exec maplay-flyway-1 flyway migrate
```

### 重置開發環境
```powershell
# 完全重置（清除所有資料）
docker-compose -f compose.yaml down -v
docker-compose -f compose.yaml up -d
```

## 腳本快速參考

### 可用腳本
```powershell
# 啟動基礎設施
.\start-infrastructure.bat

# 啟動應用程式
.\start-app.bat

# Windows 修復版本
.\start-windows-fixed.bat

# Docker 檢查
.\check-docker.bat

# WSL2 修復
.\fix-docker-wsl.ps1
```

## API 文檔相關

### 訪問 API 文檔
```powershell
# 推薦：使用 Swagger UI (穩定)
Start-Process http://localhost:8080/swagger

# 或使用 Scalar UI (現代化介面)
Start-Process http://localhost:8080/scalar/v1

# 取得 OpenAPI JSON
Invoke-WebRequest http://localhost:8080/openapi/v1.json | Select-Object -Expand Content

# 測試 API 基本連接
Test-NetConnection -ComputerName localhost -Port 8080

# 使用測試腳本
.\test-api-fixed.ps1
```

### 測試 API 端點
```powershell
# 測試註冊 API
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/auth/register" `
  -ContentType "application/json" `
  -Body '{"username":"testuser","email":"test@example.com","password":"Test1234!"}'

# 測試登入 API
Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/auth/login" `
  -ContentType "application/json" `
  -Body '{"email":"test@example.com","password":"Test1234!"}'

# 使用 Token 測試認證 API
$token = "your_token_here"
Invoke-RestMethod -Method Get -Uri "http://localhost:8080/api/auth/me" `
  -Headers @{Authorization="Bearer $token"}
```

## 注意事項

### 生產環境部署
1. 修改所有預設密碼
2. 設定適當的資源限制
3. 啟用日誌輪轉
4. 設定備份策略
5. 使用 HTTPS/TLS
6. 限制網路存取

### 資料安全
- 定期備份 volumes
- 不要在版本控制中提交 `.env` 檔案
- 使用強密碼和金鑰
- 限制容器權限

### 效能優化
- 適當調整記憶體和 CPU 限制
- 使用多階段建置減少映像檔大小
- 清理未使用的映像檔和 volumes
