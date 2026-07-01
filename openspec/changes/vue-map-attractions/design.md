## Context

目前系統具備完整的後端 API（景點管理、認證、評價等）與資料庫架構，但缺少整合前端應用程式。專案採用前後端分離架構，後端為 ASP.NET Core 8 Web API，資料庫使用 PostgreSQL 16 + PostGIS。

現有 API 端點：
- `GET /api/spots/nearby` - 附近景點查詢
- `GET /api/spots/{id}` - 景點詳情
- `POST /api/spots` - 回報新景點
- `POST /api/spots/{id}/images` - 圖片上傳
- JWT 認證相關端點

## Goals / Non-Goals

**Goals:**
- 建立現代化的 Vue 3 單頁應用程式，提供直觀的地圖操作介面
- 整合 Leaflet.js 開源地圖，無需商業 API 金鑰
- 實作完整的景點生命週期管理（查詢、檢視、回報、篩選）
- 提供響應式設計，支援桌面與行動裝置
- 使用 TypeScript 提升程式碼品質與開發體驗

**Non-Goals:**
- 不實作後端 API 修改或資料庫 schema 變更
- 不整合 Google Maps 或其他付費地圖服務
- 不實作即時協作功能
- 不支援離線地圖功能

## Decisions

### 1. 前端框架選擇：Vue 3 + Vite
**決策：** 使用 Vue 3 作為前端框架，Vite 作為建置工具

**理由：**
- Vue 3 提供更好的 TypeScript 支援與效能
- Composition API 更利於邏輯重用與程式碼組織
- Vite 提供極速的開發體驗與 HMR
- 生態系完善，地圖整合相關套件成熟

**替代方案：**
- React：生態更成熟但學習曲線較陡
- Angular：過度工程化，不符合專案規模

### 2. 狀態管理：Pinia
**決策：** 使用 Pinia 作為狀態管理工具

**理由：**
- 官方推薦的 Vuex 替代方案
- 更好的 TypeScript 支援
- 更簡潔的 API 與模組化設計

### 3. 地圖套件：Leaflet.js + OpenStreetMap
**決策：** 使用 Leaflet.js 整合 OpenStreetMap 底圖

**理由：**
- 完全開源免費，無需 API 金鑰
- 輕量級（~40KB gzipped）
- 豐富的外掛與社群支援
- 成熟穩定，廣泛使用

**替代方案：**
- Mapbox：需要付費訂閱
- Google Maps：需要 API 金鑰且有使用量限制

### 4. HTTP 客戶端：Axios
**決策：** 使用 Axios 處理 API 呼叫

**理由：**
- 自動 JSON 轉換
- 攔截器支援（統一錯誤處理、認證）
- 請求取消與重試功能
- 更好的瀏覽器相容性

### 5. TypeScript 使用
**決策：** 全面使用 TypeScript

**理由：**
- 靜態型別檢查減少 runtime 錯誤
- 更好的 IDE 支援與自動完成
- API 回應型別定義確保資料安全性
- 團隊協作時的程式碼可讀性

### 6. 專案結構
**決策：** 採用功能導向的目錄結構

```
src/
├── components/        # 可重用元件
│   ├── MapComponent.vue
│   ├── SpotPopup.vue
│   └── FilterPanel.vue
├── views/            # 頁面層級元件
│   ├── HomeView.vue
│   ├── SpotDetailView.vue
│   └── LoginView.vue
├── stores/           # Pinia 狀態管理
│   ├── auth.ts
│   ├── spots.ts
│   └── filters.ts
├── services/         # API 服務層
│   ├── apiService.ts
│   └── authSevice.ts
├── types/            # TypeScript 型別定義
│   ├── spot.ts
│   └── api.ts
└── utils/            # 工具函式
    ├── mapHelpers.ts
    └── validators.ts
```

### 7. CORS 與部署配置
**決策：** 後端配置 CORS 允許本地開發，前端支援環境變數配置

**理由：**
- 開發時前端可能運行在 `localhost:5173`，後端在 `localhost:5000`
- 生產環境可能部署至不同網域
- 使用 `.env` 檔案管理環境設定

### 8. 認證流程
**決策：** JWT Token 儲存於 localStorage，自動刷新機制

**流程：**
1. 使用者登入後取得 `accessToken` 與 `refreshToken`
2. `accessToken` 過期前自動使用 `refreshToken` 刷新
3. 刷新失敗則重導向至登入頁面
4. Axios 攔截器自動附加 `Authorization` header

### 9. 圖片上傳處理
**決策：** 使用預覽方式處理圖片選擇，上傳前驗證

**流程：**
1. 使用者選擇圖片後立即顯示預覽
2. 前端驗證檔案格式與大小
3. 上傳至 `POST /api/spots/{id}/images`
4. 上傳成功後更新景點圖片清單

## Risks / Trade-offs

### 風險 1：CORS 配置錯誤導致 API 呼叫失敗
**緩解措施：**
- 開發時明確配置允許的 origins
- 提供明確的錯誤提示指引使用者
- 文件中說明 CORS 配置方式

### 風險 2：Leaflet.js 在舊版瀏覽器相容性問題
**緩解措施：**
- 使用 Babel 轉譯 ES6+ 語法
- 加入 polyfills 支援舊版瀏覽器
- 提供基礎功能降級方案

### 風險 3：地圖效能問題（大量標記）
**緩解措施：**
- 實作標記叢集化（Leaflet.markercluster）
- 依視範圍動態載入標記
- 提供「顯示全部」與「僅顯示視範圍內」選項

### 風險 4：網路錯誤處理不完善
**緩解措施：**
- Axios 攔截器統一錯誤處理
- 提供重試機制
- 離線提示與快取策略

## Migration Plan

### 開發階段
1. 建立前端專案並配置環境
2. 實作 API 服務層與型別定義
3. 開發核心地圖元件
4. 實作認證流程
5. 完成景點管理功能
6. 整合篩選與搜尋功能
7. 響應式設計調整
8. 測試與除錯

### 部署階段
1. 前端建置為靜態檔案
2. 配置 Nginx 或其他網頁伺服器
3. 設定環境變數
4. 配置 HTTPS
5. 效能測試與優化

### 回滾策略
- 前端為新應用程式，不影響現有後端 API
- 如有問題可立即回退至上一版本
- 維持後端 API 相容性確保安全降級

## Open Questions

1. **是否需要 PWA 支援？**
   - 目前不列入目標，但可作為未來擴展方向

2. **是否需要多語言支援？**
   - 目前僅支援繁體中文，未來可擴展

3. **是否需要無障礙功能（WCAG）？**
   - 建議基礎無障礙支援，但完整符合標準可後續優化