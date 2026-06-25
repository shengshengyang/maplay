# Project: 親子資源地圖系統 (Family Resource Map)

## Mission
提供家長以地圖為核心的親子資源平台，整合公園、親子餐廳、哺乳室/廁所、活動場地、醫療等資源，支援使用者回報、管理員審查、政府開放資料匯入，並可一鍵跳轉 Google Maps 導航。

## Tech Stack
- 前端：Vue 3 + Vite + Pinia + Vue Router + Leaflet.js
- 後端：ASP.NET Core 8 Web API
- ORM：Entity Framework Core + Npgsql + NetTopologySuite
- 資料庫：PostgreSQL 16 + PostGIS
- 認證：ASP.NET Core Identity + JWT + OAuth2 (Google / Line)
- 物件儲存：S3 相容物件儲存（圖片）
- 底圖：OpenStreetMap（免費，無金鑰）
- 部署：Docker Compose（本機開發優先）

## Capabilities（能力清單）
- `auth` — 會員註冊、登入登出、JWT/Refresh、OAuth、個人資料
- `spot-management` — 景點查詢、詳情、回報、圖片上傳（物件儲存）
- `reviews` — 評價與評分（rating / clean_level）
- `admin-moderation` — 待審查清單、核准/退回、編輯、軟刪除、使用者管理
- `gov-import` — 政府開放資料批次匯入與去重
- `map-navigation` — 地圖呈現、分類標記、Google Maps 跳轉、篩選

## Roles
- `guest`：未登入，可瀏覽地圖與景點詳情、Google Maps 跳轉
- `user`：已登入，可回報景點、評價、上傳圖片
- `admin`：可審查、管理景點與使用者、匯入資料

## Conventions
- API Base：`/api`；請求/回應 JSON（檔案上傳除外）；UTF-8。
- 時間：ISO 8601 UTC；座標：WGS84 / SRID 4326。
- JSON 欄位 camelCase；DB 欄位 snake_case。
- 統一成功/失敗回應與 `errorCode` 錯誤碼；例外集中於中介層處理。
- 清單端點一律強制分頁（page/pageSize，pageSize 上限 100）。
- 權限後端強制（`[Authorize]` / `[Authorize(Roles="Admin")]`），前端守衛僅體驗層。

## Key Design Decisions
- 圖片儲存採 S3 相容**物件儲存**，後端以儲存介面抽象，業務碼不直接依賴特定供應商。
- 景點刪除採**軟刪除**（`deleted_at` 標記），保留評價與稽核；公開查詢與審查清單一律排除已刪除。
- 期間限定景點以 `spot_type=temporary` + `start_date`/`end_date` 表示，過期自動不顯示。
- 政府匯入資料以 `gov_data_id` 去重，匯入後直接 `approved`。

## Out of Scope（本期不實作，僅預留）
- 爬蟲自動發掘景點（Phase 2，DB 已預留 `source_url`/`crawled_at`）。
- AI 行程規劃（Phase 3，預留 `POST /api/ai/plan-itinerary`）。
- 付費金流、推播通知。
