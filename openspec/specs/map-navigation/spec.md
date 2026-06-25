# map-navigation Specification

## Purpose
前端地圖呈現與互動：OpenStreetMap 底圖、景點分類標記、期間限定標示、Google Maps 導航跳轉、分類與年齡篩選。

## Requirements

### Requirement: 地圖底圖與初始定位
系統 SHALL 以 Leaflet + OpenStreetMap tile 呈現地圖（免費、無金鑰）。初始定位 SHALL 嘗試 `navigator.geolocation`，失敗時 MUST 回退至預設台中座標（24.1477, 120.6736）。

#### Scenario: 取得使用者定位
- **WHEN** 使用者允許定位
- **THEN** 地圖中心設為使用者目前位置

#### Scenario: 定位失敗回退
- **WHEN** 使用者拒絕定位或取得失敗
- **THEN** 地圖中心回退至台中預設座標，且不顯示錯誤中斷

### Requirement: 景點分類標記
系統 SHALL 依景點 category 顯示對應圖示；spotType=temporary 之標記 MUST 額外疊加期間限定標示（如 ⏰）。

#### Scenario: 分類圖示對應
- **GIVEN** 不同分類的景點
- **WHEN** 載入地圖標記
- **THEN** 各景點依其 category 顯示對應圖示

#### Scenario: 期間限定額外標示
- **GIVEN** 一筆 temporary 景點
- **WHEN** 顯示其標記
- **THEN** 標記帶有期間限定視覺標示

### Requirement: Google Maps 導航跳轉
系統 SHALL 於景點 Popup 與詳情頁提供「在 Google Maps 開啟」操作，開啟 `https://maps.google.com/?q={lat},{lng}&query={name}`，MUST NOT 需要任何 API 金鑰。行動裝置已安裝 App 時應開啟 App，否則開啟網頁；桌機開新分頁。

#### Scenario: 跳轉座標正確
- **WHEN** 使用者於某景點點擊「在 Google Maps 開啟」
- **THEN** 開啟之 Google Maps 目標座標與該景點座標一致

#### Scenario: 無需金鑰
- **WHEN** 系統未設定任何地圖 API 金鑰
- **THEN** 跳轉功能仍可正常運作

### Requirement: 篩選面板
系統 SHALL 提供分類多選、年齡層多選與永久/期間限定切換之篩選；變更條件後 MUST 以新條件重新查詢 `GET /api/spots/nearby`。

#### Scenario: 套用分類篩選
- **WHEN** 使用者勾選特定分類
- **THEN** 地圖僅顯示符合分類的景點

#### Scenario: 套用年齡層篩選
- **WHEN** 使用者勾選特定年齡層
- **THEN** 地圖僅顯示 ageGroups 含該年齡層的景點

### Requirement: 空狀態與錯誤呈現
系統 SHALL 於無查詢結果、API 失敗或定位失敗時呈現明確的空狀態或錯誤提示，MUST NOT 以空白畫面或未處理錯誤呈現。

#### Scenario: 範圍內無景點
- **WHEN** 查詢結果為空
- **THEN** 顯示「此範圍尚無資源」之空狀態，並引導使用者擴大範圍或回報景點
