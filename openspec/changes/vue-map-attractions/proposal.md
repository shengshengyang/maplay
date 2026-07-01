## Why

目前系統已有後端 API 和地圖導航規格，但缺少整合前端應用程式。需要建立 Vue 3 地圖應用程式，讓使用者能直觀地在開源地圖上檢視、標註和管理親子景��，提升使用體驗。

## What Changes

- 建立 Vue 3 + Vite 專案，整合 Leaflet.js 開源地圖（使用 OpenStreetMap 底圖）
- 實作地圖互動功能：標記點位、檢視景點資訊、彈跳視窗顯示
- 串接現有 C# API（景點 CRUD、圖片上傳、使用者認證）
- 支援使用者在地圖上新增景點（包含座標拾取）
- 實作景點分類篩選（公園、餐廳、哺乳室、醫療等）

## Capabilities

### New Capabilities
- `vue-map-app`: Vue 3 地圖應用程���，提供互動式地圖介面和景點標註功能，整合現有後端 API

### Modified Capabilities
- `spot-management`: 前端需支援地圖式景點管理介面（無需修改後端規格）
- `map-navigation`: 前端需整合開源地圖導航功能（無需修改後端規格）

## Impact

**新增依賴**：
- 前端框架：Vue 3、Vite、Pinia（狀態管理）、Vue Router
- 地圖套件：Leaflet.js、OpenStreetMap 底圖���無需 API 金鑰）
- HTTP 客戶端：Axios 或 fetch API

**影響範圍**：
- 建立全新前端專案（獨立於後端 API）
- 需配置 CORS 允許前端呼叫 API
- 部署時需提供靜態檔案服務

**非破壞性變更**：此為新增前端應用程式，不影響現有後端 API 運作

## Non-goals (本期不做)

- 不實作 Google Maps 整合
- 不更動後端 API 規格或資料庫 schema
- 不實作複雜的地圖分析或路徑規劃功能
- 不支援離線地圖功能
- 不實作即時協作編輯