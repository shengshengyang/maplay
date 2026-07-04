## ADDED Requirements

### Requirement: API 文件介面可用性
系統 SHALL 提供互動式 API 文件介面，讓開發者與測試人員能夠瀏覽、測試 API 端點。

#### Scenario: 開發環境訪問 Swagger UI
- **WHEN** 應用程式於開發環境啟動
- **THEN** Swagger UI 介面於 /swagger 路徑可訪問
- **AND** 介面顯示所有公開的 API 端點
- **AND** 介面支援互動式 API 測試

#### Scenario: 生產環境停用 Swagger UI
- **WHEN** 應用程式於生產環境啟動
- **THEN** Swagger UI 介面不可訪問
- **AND** 訪問 /swagger 路徑回傳 404 Not Found
- **AND** OpenAPI 規格端點亦停用

#### Scenario: Swagger UI 基本功能
- **WHEN** 使用者訪問 Swagger UI 介面
- **THEN** 可瀏覽所有 API 端點的 HTTP 方法與路徑
- **AND** 可查看每個端點的請求參數與回應格式
- **AND** 可直接在介面中執行 API 測試

### Requirement: OpenAPI 規格產生
系統 SHALL 自動產生符合 OpenAPI 3.0 規格的 API 文件，描述所有公開端點的結構。

#### Scenario: OpenAPI 規格端點提供
- **WHEN** 應用程式於開發環境啟動
- **THEN** /swagger/v1/swagger.json 端點提供 OpenAPI 3.0 規格
- **AND** 規格包含所有 API 端點的完整資訊
- **AND** 規格可用於客戶端 SDK 產生工具

#### Scenario: API 資訊完整性
- **WHEN** OpenAPI 規格產生時
- **THEN** 包含 API 基本資訊 (標題、版本、描述)
- **AND** 包含所有端���的 HTTP 方法、路徑、參數、回應格式
- **AND** 包含資料模型定義與 schema
- **AND** 包含認證方式說明

### Requirement: JWT 認證測試支援
系統 SHALL 在 Swagger UI 中支援 JWT Bearer Token 認證，讓開發者可測試需要授權的 API 端點。

#### Scenario: Swagger UI 設定認證 Token
- **WHEN** 使用者在 Swagger UI 中設定 JWT Bearer Token
- **THEN** 可在介面中輸入有效的 JWT Token
- **AND** Token 儲存於瀏覽器本地，後續請求自動附加 Authorization header
- **AND** Token 格式為 "Bearer {jwt_token}"

#### Scenario: 測試需要認證的端點
- **WHEN** 使用者設定 JWT Token 後測試受保護的 API 端點
- **THEN** Swagger UI 在請求中附加 Authorization: Bearer {token} header
- **AND** API 正確驗證 Token 並執行請求
- **AND** 若 Token 無效，API 回傳 401 Unauthorized 狀態碼

#### Scenario: 未授權測試公開端點
- **WHEN** 使用者未設定 Token 而測試公開 API 端點
- **THEN** Swagger UI 正常執行請求
- **AND** API 正確回應資料

### Requirement: API 文件說明與註解
系統 SHALL 提供��楚的 API 說明、參數描述、回應範例，幫助開發者理解如何使用各端點。

#### Scenario: API 端點分組顯示
- **WHEN** Swagger UI 顯示 API 端點
- **THEN** 端點按功能分組 (如 Authentication, Spots, Reviews, Admin)
- **AND** 每個分組有清楚的標題與描述
- **AND** 相關端點聚集於同一分組

#### Scenario: XML 註解轉換為 API 文件
- **WHEN** API 控制器與模型包含 XML 註解
- **THEN** 註解自動轉換為 Swagger UI 說明文字
- **AND** 包含端點描述、參數說明、回應格式描述
- **AND** 包含資料模型欄位說明與驗證規則

#### Scenario: 回應範例展示
- **WHEN** Swagger UI 顯示 API 端點
- **THEN** 每個端點顯示預期回應格式與範例
- **AND** 標示不同 HTTP 狀態碼的回應 (200, 400, 401, 404, 500)
- **AND** 提供錯誤回應的 errorCode 與訊息範例

### Requirement: 環境隔離配置
系統 SHALL 根據執行環境自動啟用或停用 API 文件功能，確保生產環境安全。

#### Scenario: 開發環境啟用 Swagger
- **WHEN** ASPNETCORE_ENVIRONMENT 設定為 Development
- **THEN** Swagger UI 與 OpenAPI 端點完全啟用
- **AND** 介面提供詳細的 API 說明與測試功能

#### Scenario: 生產環境停用 Swagger
- **WHEN** ASPNETCORE_ENVIRONMENT 設定為 Production
- **THEN** Swagger UI 與 OpenAPI 端點完全停用
- **AND** 嘗試訪問 /swagger 路徑回傳 404
- **AND** 嘗試訪問 /swagger/v1/swagger.json 回傳 404

#### Scenario: 自訂環境配置
- **WHEN** 應用程式於其他環境啟動 (如 Staging)
- **THEN** 可透過配置檔案控制 Swagger 啟用狀態
- **AND** 支援強制啟用或停用，不依賴環境變數

### Requirement: 相容性與整合性
系統 SHALL 與現有 ASP.NET Core 8 架構完美整合，不影響既有 API 行為或效能。

#### Scenario: 無干擾既有 API 功能
- **WHEN** Swagger 整合至應用程式
- **THEN** 既有 API 端點行為完全不受影響
- **AND** API 認證授權邏輯維持不變
- **AND** API 回應格式與內容維持不變

#### Scenario: Swagger 中介層效能影響
- **WHEN** Swagger 已啟用的應用程式處理 API 請求
- **THEN** Swagger 中介層不產生可見的效能影響
- **AND** API 回應時間與未啟用 Swagger 時相當

#### Scenario: NuGet 套件相容性
- **WHEN** 專案引用 Swashbuckle.AspNetCore 套件
- **THEN** 套件與 ASP.NET Core 8 完全相容
- **AND** 套件不與現有 NuGet 套件產生衝突