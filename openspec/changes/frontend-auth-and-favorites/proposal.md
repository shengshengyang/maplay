## Why

目前前端應用缺少用戶認證與收藏功能，用戶無法登入、個人化體驗或追蹤感興趣的景點。後端已提供完整的認證 API（JWT + OAuth）與收藏端點，需要實現前端對接以提升用戶參與度。

## What Changes

- **新增前端認證功能**：實現登入/登出流程，支援 Email+Password 與 OAuth（Google/Line）
- **新增景點收藏功能**：登入用戶可收藏景點、查看個人收藏列表、檢查收藏狀態
- **調整路由與狀態管理**：認證守衛保護需登入頁面，Pinia store 管理認證與收藏狀態
- **UI 組件**：登入表單、收藏按鈕、收藏列表頁面

## Capabilities

### New Capabilities
- `frontend-auth`: 前端用戶認證功能，包含登入、登出、OAuth 回調、認證狀態管理
- `frontend-favorites`: 前端景點收藏功能，包含收藏操作、收藏列表、收藏狀態顯示

### Modified Capabilities
- 無（後端 API 規格不變，僅前端實作）

## Impact

**前端：**
- 新增 Pinia stores：authStore, favoritesStore
- 新增頁面/組件：LoginView, FavoriteButton, FavoritesView
- 新增 API 客戶端函數：auth API, favorites API
- 路由守衛調整：保護需認證頁面

**依賴：**
- 後端 `/api/auth/*` 端點（登入、註冊、OAuth）
- 後端 `/api/favorites/*` 端點（收藏 CRUD）

**風險：**
- OAuth 回調處理需正確配置 redirect URI
- JWT token 過期需自動重新整理或引導重新登入
- 收藏狀態需與後端同步，避免樂觀更新導致的不一致

## Non-goals (本期不做)

- 社群分享收藏列表
- 收藏分類/標籤功能
- 收藏統計與推薦
- 忘記密碼流程（後期優化）
- 個人資料編輯頁面
