# vue-map-app Specification

## Purpose
定義 Vue 3 地圖應用程式的功能需求，整合現有後端 API 與 Leaflet.js 開源地圖，提供使用者直觀的親子景點標註與管理介面。

## ADDED Requirements

### Requirement: Vue 3 專案架構
系統 SHALL 使用 Vue 3 + Vite 建立前端應用程式，使用 Pinia 進行狀態管理，Vue Router 處理路由。專案結構 SHALL 包含 components/、views/、stores/、services/ 目錄。

#### Scenario: 專案初始化完成
- **WHEN** 建立新的 Vue 3 專案
- **THEN** 專案可正常啟動，開發伺服器運行在指定埠號

#### Scenario: 核心依賴套件安裝
- **WHEN** ��裝 Vue 3、Vite、Pinia、Vue Router、Leaflet
- **THEN** 所有套件在 package.json 中正確配置，無版本衝突

### Requirement: 地圖元件整合
系統 SHALL 提供 `MapComponent.vue` 元件，整合 Leaflet.js 與 OpenStreetMap 底圖。元件 SHALL 支援標記顯示、彈跳視窗、座標拾取功能。

#### Scenario: 地圖��常渲染
- **WHEN** 載入 MapComponent 元件
- **THEN** 顯示 OpenStreetMap 底圖，地圖可正常縮放和平移

#### Scenario: 標記顯示景點
- **WHEN** 傳入景點資料陣列
- **THEN** 地圖上顯示對應數量的標記，標記位置正確

#### Scenario: 彈跳視窗顯示景點資訊
- **WHEN** 點擊景點標記
- **THEN** 顯示彈跳視窗，包含景點名稱、分類、地址等基本資訊

### Requirement: API 服務層
系統 SHALL 提供 `apiService.js` 處理所有後端 API 呼叫，使用 axios 或 fetch API。服務層 SHALL 包含錯誤處理、認證 token 管理、請求重試邏輯。

#### Scenario: 呼叫附近景點 API
- **WHEN** 呼叫 `getNearbySpots(lat, lng, radius, filters)`
- **THEN** 正確呼叫 `GET /api/spots/nearby`，回傳景點資料陣列

#### Scenario: 處理認證錯誤
- **WHEN** API ���傳 401 未授權錯誤
- **THEN** 自動嘗試刷新 token 或重導向至登入頁面

#### Scenario: 處理網路錯誤
- **WHEN** API 呼叫失敗或超時
- **THEN** 顯示使用者友好的錯誤提示，提供重試選項

### Requirement: 景點標註功能
系統 SHALL 允許使用者在地圖上點擊以新增景點，收集座標資訊並填入表單。表單 SHALL 包含名稱、分類、地址、適合年齡、設施標籤等欄位。

#### Scenario: 地圖點擊取得座標
- **WHEN** 使用者在「新增模式」下點擊地圖
- **THEN** 取得點擊位置的經緯度座標，並顯示於表單中

#### Scenario: 表單驗證
- **WHEN** 提交景點資料時缺少必填欄位
- **THEN** 顯示驗證錯誤，阻止表單提交

#### Scenario: 成功提交景點
- **WHEN** 使用者填寫完整景點資料並提交
- **THEN** 呼叫 `POST /api/spots` API，顯示成功訊息並在地圖上顯示新標記

### Requirement: 分類篩選介面
系統 SHALL 提供篩選面板，支援依分類、年齡層、景點類型篩選景點。篩選條件變更時，系統 SHALL 自動重新查詢 API 並更新地圖標記。

#### Scenario: 分類篩選
- **WHEN** ��用者勾選或取消勾選特定分類
- **THEN** 地圖僅顯示符合選定分類的景點標記

#### Scenario: 年齡層篩選
- **WHEN** 使用者選擇特定年齡層
- **THEN** 地圖僅顯示包含該年齡層的景點標記

#### Scenario: 清除篩選
- **WHEN** 使用者重置所有篩選條件
- **THEN** 顯��所有景點標記

### Requirement: 認證狀態管理
系統 SHALL 使用 Pinia store 管理使用者認證狀態，儲存 accessToken、refreshToken、使用者資訊。Store SHALL 提供 login、logout、refreshToken 等操作。

#### Scenario: 登入狀態持久化
- **WHEN** 使用者登入成功
- **THEN** 認證資訊儲存於 localStorage，重新載入頁面時保持登入狀態

#### Scenario: 自動 Token 刷新
- **WHEN** accessToken 即將過期
- **THEN** 自動使用 refreshToken 取得新的 accessToken

### Requirement: 響應式設計
系�� SHALL 支援桌面與行動裝置，地圖與控制面板在不同螢幕尺寸下皆可正常操作。行動版 SHALL 提供觸控友善的介面。

#### Scenario: 桌面版介面
- **WHEN** 在桌面瀏覽器檢視
- **THEN** 地圖與控制面板並排顯示，操作按鈕適合滑鼠點擊

#### Scenario: 行動版介面
- **WHEN** 在行動��置檢視
- **THEN** 控制面板調整為底部或抽屜式設計，按鈕尺寸適合觸控操作

### Requirement: 錯誤處理與使用者提示
系統 SHALL 在各種錯誤情況下提供明確的使用者提示，包含網路錯誤、API 錯誤、表單驗證錯誤等。提示訊息 SHALL 使用繁體中文。

#### Scenario: 網路連線失敗
- **WHEN** 裝置無網路連線
- **THEN** 顯示「網路連線失敗，請檢查您的網路設定」提示

#### Scenario: API 回傳錯誤
- **WHEN** API 回傳 4xx 或 5xx 錯誤
- **THEN** 顯示對應的中文錯誤訊息，根據錯誤類型提供解決建議

#### Scenario: 表單驗證錯誤
- **WHEN** ��單欄位驗證失敗
- **THEN** 在對應欄位下方顯示具體的錯誤訊息

### Requirement: CORS 與部署配置
系統 SHALL 正確配置 CORS，允許前端應用程式呼叫後端 API。部署配置 SHALL 支援 Docker Compose 本地開發���生產環境部署。

#### Scenario: 本地開發 CORS
- **WHEN** 前端運行在開發伺服器，後端運行在不同埠號
- **THEN** API 呼叫不受 CORS 限制，正常取得資料

#### Scenario: Docker Compose 部署
- **WHEN** 使用 Docker Compose 啟動完整服務
- **THEN** 前端可正常呼叫後端 API，應用程式完全運作