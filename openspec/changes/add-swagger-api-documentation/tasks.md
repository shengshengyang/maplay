## 1. 套件安裝與專案配置

- [x] 1.1 安裝 Swashbuckle.AspNetCore NuGet 套件
- [x] 1.2 啟用 XML 文件產生於專案檔 (.csproj)
- [x] 1.3 設定 XML 文件輸出路徑與包含項目
- [x] 1.4 驗證套件安裝成功與專案編譯無誤

**驗證：** `dotnet build` 成功，專案可正常執行

## 2. Swagger 基礎配置

- [x] 2.1 於 Program.cs 註冊 Swagger 服務 (開發環境)
- [x] 2.2 配置 Swagger 端點基本資訊 (標題、版本、描述)
- [x] 2.3 啟用 Swagger UI 中介層
- [x] 2.4 設定 OpenAPI 規格版本為 3.0
- [ ] 2.5 驗證 /swagger 端點可訪問並顯示基本介面

**驗證：** 開發環境訪問 `http://localhost:8080/swagger` 顯示 Swagger UI

## 3. JWT 認證整合

- [ ] 3.1 新增 Bearer Token 安全定義至 Swagger 配置
- [ ] 3.2 設定 JWT Token 輸入介面於 Swagger UI
- [ ] 3.3 配置全域安全需求至所有端點
- [ ] 3.4 測試 JWT Token 認證功能 (使用有效 Token)
- [ ] 3.5 驗證無效 Token 回傳 401 Unauthorized

**驗證：** Swagger UI 可設定 JWT Token 並成功測試受保護端點

## 4. XML 文件整合

- [ ] 4.1 於主要 API 控制器加入 XML 註解
- [ ] 4.2 於 DTO 模型加入屬性說明註解
- [ ] 4.3 配置 SwaggerGen 包含 XML 註解檔案
- [ ] 4.4 驗證 XML 註解顯示於 Swagger UI

**驗證：** Swagger UI 顯示端點與參數的 XML 註解說明

## 5. API 端點分組與註解

- [ ] 5.1 為認證相關控制器設定 "Authentication" 分組
- [ ] 5.2 為景點管理控制器設定 "Spot Management" 分組
- [ ] 5.3 為評價相關控制器設定 "Reviews" 分組
- [ ] 5.4 為管理功能控制器設定 "Admin Moderation" 分組
- [ ] 5.5 為所有公開端點加入 HTTP 方法與參數說明
- [ ] 5.6 為回應模型加入欄位說明與驗證規則註解

**驗證：** Swagger UI 端點按功能分組顯示，包含完整說明

## 6. 環境隔離配置

- [ ] 6.1 設定條件編譯，Swagger 僅於開發環境註冊
- [ ] 6.2 驗證生產環境 (ASPNETCORE_ENVIRONMENT=Production) 停用 Swagger
- [ ] 6.3 測試 Docker Compose 開發環境配置
- [ ] 6.4 確認生產環境訪問 /swagger 回傳 404

**驗證：** 開發環境 Swagger 啟用，生產環境停用

## 7. 進階配置與客製化

- [ ] 7.1 設定 API 基礎資訊 (專案描述、聯絡資訊)
- [ ] 7.2 配置 Swagger UI 介面樣式與主題
- [ ] 7.3 設定不同 HTTP 狀態碼的回應範例
- [ ] 7.4 加入 OAuth2 流程說明 (如 Google/Line 登入)
- [ ] 7.5 配置模型顯示規則 (camelCase 處理)

**驗證：** Swagger UI 顯示完整的 API 資訊與回應範例

## 8. 測試與驗證

- [ ] 8.1 測試所有公開端點於 Swagger UI 的執行功能
- [ ] 8.2 測試需要認證端點的 JWT Token 測試流程
- [ ] 8.3 驗證 OpenAPI 規格端點 (/swagger/v1/swagger.json)
- [ ] 8.4 檢查 XML 註解正確顯示於文件中
- [ ] 8.5 測試不同資料類型的序列化與顯示
- [ ] 8.6 驗證分頁參數與篩選條件的測試介面

**驗證：** 所有功能可通過 Swagger UI 正常測試

## 9. 文件與部署

- [ ] 9.1 更新專案 README，加入 Swagger 文件連結
- [ ] 9.2 更新開發者手冊，說明 Swagger 使用方式
- [ ] 9.3 建立 JWT Token 測試指南文件
- [ ] 9.4 設定 Docker Compose 生產環境配置
- [ ] 9.5 驗證 Docker 部署後 Swagger 功能正常

**驗證：** 開發者可透過文件快速使用 Swagger 進行 API 測試

## 10. 效能與安全性檢查

- [ ] 10.1 測量 Swagger 中介層對 API 效能的影響
- [ ] 10.2 驗證生產環境配置完全停用 Swagger
- [ ] 10.3 檢查 JWT Token 於 Swagger UI 的儲存安全性
- [ ] 10.4 確認無敏感資訊洩漏至 OpenAPI 規格中
- [ ] 10.5 測試 Swagger 對既有 API 行為無影響

**驗證：** 效能無顯著影響，安全性符合要求