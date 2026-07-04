## Why

目前專案缺乏 API 文件與測試介面，開發者與前端團隊無法快速了解 API 端點、請求格式、回應結構，且缺乏即時測試工具。加入 Swagger/OpenAPI 可提供互動式 API 文件，��升開發效率與協作品質。

## What Changes

- 整合 Swashbuckle.AspNetCore 至 ASP.NET Core 8 Web API
- 配置 Swagger 產生 OpenAPI 3.0 規格文件
- 建立互動式 API 測試 UI (Swagger UI)
- 支援 JWT Bearer Token 認證測試
- 提供 API 端點分群與說明註解
- 環境隔離：開發環境啟用，生產環境停用

## Capabilities

### New Capabilities
- `api-documentation`: API 文件與測試介面，包含 Swagger UI 整合、OpenAPI 規格產生、JWT 認證支援、環境隔離配置

### Modified Capabilities
(無 - 此變更為新增功能，不修改既有能力需求)

## Impact

**受影響系統：**
- ASP.NET Core Web API 專案配置
- NuGet 套件依賴 (新增 Swashbuckle.AspNetCore)
- 開發環境中介層註冊
- 生產環境部署配置

**風險評估：**
- 低風險：僅新增文件功能，不影響既有 API 行為
- 安全性：生產環境停用 Swagger，避免暴露敏感資訊
- 相容性：Swashbuckle 完美支援 ASP.NET Core 8

## Non-goals (本期不做)

- 不修改既有 API 端點行為或回應格式
- 不變更認證授權邏輯，僅於 Swagger UI 測試支援
- 不建立自動化 API 測試，僅提供手動測試介面
- 不產生客戶端 SDK，僅提供 OpenAPI 規格文件