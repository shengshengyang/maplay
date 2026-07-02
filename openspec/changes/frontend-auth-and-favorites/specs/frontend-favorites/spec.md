# frontend-favorites Specification

## Purpose
實現前端景點收藏功能，讓登入用戶可以收藏景點、查看收藏列表、檢查收藏狀態。

## ADDED Requirements

### Requirement: 收藏按鈕顯示
系統 SHALL 在景點詳情頁顯示收藏按鈕，已登入用戶顯示收藏狀態（已收藏/未收藏），未登入時隱藏或顯示「登入後收藏」。

#### Scenario: 已登入用戶查看景點詳情
- **GIVEN** 用戶已登入
- **WHEN** 用戶訪問景點詳情頁
- **THEN** 系統顯示收藏按鈕
- **AND** 呼叫 GET /api/favorites/check/{spotId} 檢查收藏狀態
- **AND** 依據結果顯示「已收藏」或「未收藏」狀態

#### Scenario: 未登入用戶查看景點詳情
- **GIVEN** 用戶未登入
- **WHEN** 用戶訪問景點詳情頁
- **THEN** 系統顯示「登入後可收藏」提示
- **OR** 系統隱藏收藏按鈕

#### Scenario: 收藏狀態載入中
- **GIVEN** 正在檢查收藏狀態
- **WHEN** API 呼叫進行中
- **THEN** 收藏按鈕顯示載入中狀態（如轉圈圖示）

### Requirement: 添加收藏
系統 SHALL 允許登入用戶點擊收藏按鈕收藏景點，成功後更新按鈕狀態並顯示提示。

#### Scenario: 成功收藏景點
- **GIVEN** 用戶已登入且未收藏該景點
- **WHEN** 用戶點擊「收藏」按鈕
- **THEN** 系統立即更新按鈕為「已收藏」狀態（樂觀更新）
- **AND** 呼叫 POST /api/favorites
- **AND** API 成功後顯示提示「已收藏」
- **AND** API 失敗則回滾狀態並顯示錯誤

#### Scenario: 收藏已收藏的景點
- **GIVEN** 用戶已收藏該景點
- **WHEN** 用戶再次點擊收藏按鈕
- **THEN** 系統顯示提示「已收藏過此景點」
- **AND** 不發送 API 請求

#### Scenario: 收藏失敗
- **GIVEN** API 呼叫失敗（如網路錯誤）
- **WHEN** 用戶點擊「收藏」按鈕
- **THEN** 系統回滾按鈕狀態為「未收藏」
- **AND** 顯示錯誤提示（「收藏失敗，請稍後再試」）

### Requirement: 取消收藏
系統 SHALL 允許登入用戶取消收藏，成功後更新按鈕狀態並顯示提示。

#### Scenario: 成功取消收藏
- **GIVEN** 用戶已登入且已收藏該景點
- **WHEN** 用戶點擊「取消收藏」按鈕
- **THEN** 系統立即更新按鈕為「未收藏」狀態（樂觀更新）
- **AND** 呼叫 DELETE /api/favorites/{spotId}
- **AND** API 成功後顯示提示「已取消收藏」
- **AND** API 失敗則回滾狀態並顯示錯誤

#### Scenario: 取消收藏失敗
- **GIVEN** API 呼叫失敗
- **WHEN** 用戶點擊「取消收藏」按鈕
- **THEN** 系統回滾按鈕狀態為「已收藏」
- **AND** 顯示錯誤提示

### Requirement: 個人收藏列表
系統 SHALL 提供個人收藏列表頁面，顯示用戶收藏的所有景點，支持分頁與快速取消收藏。

#### Scenario: 查看收藏列表
- **GIVEN** 用戶已登入且有收藏景點
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統呼叫 GET /api/favorites 取得收藏列表
- **AND** 顯示景點列表（包含景點名稱、分類、縮圖、收藏時間）
- **AND** 按收藏時間倒序排列

#### Scenario: 收藏列表分頁
- **GIVEN** 用戶收藏超過 20 個景點
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統顯示前 20 筆收藏
- **AND** 顯示分頁控制（上一頁、下一頁、頁碼）
- **AND** 用戶點擊分頁控制載入對應頁面

#### Scenario: 空收藏列表
- **GIVEN** 用戶從未收藏任何景點
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統顯示「還沒有收藏任何景點」提示
- **AND** 提供連結至景點列表頁面

#### Scenario: 收藏列表載入失敗
- **GIVEN** API 呼叫失敗
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統顯示錯誤提示
- **AND** 提供重試按鈕

#### Scenario: 從收藏列表快速取消收藏
- **GIVEN** 用戶在收藏列表頁面
- **WHEN** 用戶點擊景點卡片的「取消收藏」按鈕
- **THEN** 系統立即移除該景點卡片（樂觀更新）
- **AND** 呼叫 DELETE /api/favorites/{spotId}
- **AND** API 失敗則顯示錯誤並恢復卡片

### Requirement: 收藏狀態同步
系統 SHALL 在用戶登入時重新載入收藏狀態，確保前後端一致。

#### Scenario: 登入後同步收藏狀態
- **GIVEN** 用戶在未登入狀態下瀏覽景點
- **WHEN** 用戶完成登入
- **THEN** 系統重新載入當前頁面的收藏狀態
- **AND** 更新收藏按鈕顯示

#### Scenario: 切換頁面時檢查收藏狀態
- **GIVEN** 用戶已登入
- **WHEN** 用戶從景點 A 詳情頁切換至景點 B 詳情頁
- **THEN** 系統呼叫 GET /api/favorites/check/{spotId} 檢查景點 B 的收藏狀態
- **AND** 更新收藏按鈕顯示
