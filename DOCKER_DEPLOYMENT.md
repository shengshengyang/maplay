# Docker 部署說明

## 檔案結構

- `compose.yaml` - 完整版本（包含所有服務）
- `compose-infrastructure.yaml` - 基礎設施版本（PostgreSQL + MinIO）
- `compose-app.yaml` - 應用程式版本（Maplay 後端）

## 部署方式

### 方式一：使用完整版本（適合本地開發）

```bash
# 一次啟動所有服務
docker-compose -f compose.yaml up -d

# 停止所有服務
docker-compose -f compose.yaml down

# 重建並啟動
docker-compose -f compose.yaml up -d --build
```

### 方式二：分離部署（適合生產/更新部署）

```bash
# 第一步：啟動基礎設施（只做一次或基礎設施更新時）
docker-compose -f compose-infrastructure.yaml up -d

# 第二步：啟動應用程式（每次程式更新時）
docker-compose -f compose-app.yaml up -d --build

# 只更新應用程式（不改到基礎設施）
docker-compose -f compose-app.yaml up -d --build maplay

# 停止應用程式（保留基礎設施）
docker-compose -f compose-app.yaml down

# 停止基礎設施（會移除資料庫和 MinIO）
docker-compose -f compose-infrastructure.yaml down
```

## 密碼設定

所有服務的密碼已確認一致：

| 服務 | 密碼 | 使用位置 |
|------|------|----------|
| PostgreSQL | `maplay` | db, flyway, maplay |
| MinIO | `maplay_minio_secure_password_2024` | minio, minio-init, maplay |

注意：MinIO 密碼已更改以避免使���預設密碼的安全性警告

## 網路設定

服務之間透過名為 `maplay-network` 的 Docker 網路通訊：
- `compose-infrastructure.yaml` 會建立網路
- `compose-app.yaml` 會連接到現有網路

## 健康檢查

- PostgreSQL：`pg_isready -U maplay -d maplay`
- MinIO：`mc ready local`
- 應用程式會等 Flyway 完成和 MinIO 就緒後才啟動

## 故障排除

### PostgreSQL 容器崩潰 (exit code 139 / Segmentation fault)

這是 Windows Docker Desktop 和 PostgreSQL 容器的已知相容性問題。

**快速修復步驟：**

1. **清除所有容器和資料**：
   ```powershell
   # 停止所有容器
   docker-compose -f compose.yaml down
   # 清理 volumes
   docker volume prune
   ```

2. **使用 Windows 修復版本**：
   ```powershell
   docker-compose -f compose-fix-windows.yaml up -d
   ```

3. **如果還是失敗，運行修復腳本**：
   ```powershell
   .\fix-docker-wsl.ps1
   ```

**根本解決方案：**

1. **檢查 Docker Desktop 設定**：
   - 開啟 Docker Desktop
   - Settings → General：確認使用 **WSL 2 引擎**
   - Settings → Resources → Memory：至少 **4GB**
   - Settings → Resources → WSL Integration：**啟用**
   - 重啟 Docker Desktop

2. **重置 WSL2 網路**：
   ```powershell
   wsl --shutdown
   # 然後重新啟動 Docker Desktop
   ```

3. **使用不同的 PostgreSQL 版本**：
   ```powershell
   # 嘗試標準版本（無 PostGIS）
   docker-compose -f compose-infrastructure-alt.yaml up -d

   # 或使用 Windows 修復版本（PostgreSQL 14）
   docker-compose -f compose-fix-windows.yaml up -d
   ```

**可用的版本：**
- `compose.yaml` - 標準版本（PostgreSQL 16 + PostGIS）
- `compose-fix-windows.yaml` - Windows 相容版本（PostgreSQL 14 + PostGIS）
- `compose-infrastructure-alt.yaml` - 輕量版本（PostgreSQL 16，無 PostGIS）

### MinIO 預設密碼警告

新版本已使用安全密碼，如仍看到警告：
- 確保使用最新的 compose-infrastructure.yaml
- 舊的 MinIO 容器可能仍使用預設密碼

### 應用程式無法連接資料庫

1. 確認基礎設施服務正常運行：
   ```bash
   docker-compose -f compose-infrastructure.yaml ps
   ```

2. 檢查網路連接：
   ```bash
   docker network inspect maplay_maplay-network
   ```

3. 查看應用程式日誌：
   ```bash
   docker-compose -f compose-app.yaml logs maplay
   ```

## API 文檔使用

應用程式啟動後，可以訪問互動式 API 文檔：

### Swagger UI (推薦 - 穩定)
```
http://localhost:8080/swagger
```
提供經典且穩定的 API 測試介面，支援：
- 完整的 API 測試功能
- JWT Token 認證
- 請求/回應範例
- 優異的瀏覽器相容性

### Scalar UI (現代化介面)
```
http://localhost:8080/scalar/v1
```
提供現代化的 API 測試介面，支援：
- 簡潔的使用者介面
- 即時測試 API 請求
- 響應式設計

### OpenAPI JSON
```
http://localhost:8080/openapi/v1.json
```
可用於生成客戶端 SDK 或整合其他工具。

詳細使用說明請參考 [OPENAPI_GUIDE.md](OPENAPI_GUIDE.md)。

## 注意事項

1. 生產環境請修改 `compose-app.yaml` 中的環境變數（JWT Secret、OAuth 設定等）
2. 資料庫資料會儲存在 Docker volume `pgdata` 中
3. MinIO 資料會儲存在 Docker volume `miniodata` 中
4. 停止基礎設施會保留 volumes，如需清除請加上 `-v` 參數：
   ```bash
   docker-compose -f compose-infrastructure.yaml down -v
   ```
