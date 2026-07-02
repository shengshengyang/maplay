## 1. 專案設置與基礎架構

- [x] 1.1 安裝並配置 Pinia
  - 在 Vue 專案中安裝 pinia 與 vue-router
  - 創建 stores 目錄結構
  - 配置 Pinia 到 main.js
  - **驗證：** 專案正常啟動，Pinia devTools 顯示

- [x] 1.2 配置路由與認證守衛
  - 設置路由 /login、/auth/callback/:provider、/favorites
  - 創建 requireAuth 路由守衛
  - 配置未登入重導向邏輯
  - **驗證：** 訪問 /favorites 未登入時重導向至 /login

- [x] 1.3 建立 API 客戶端基礎
  - 創建 api/ 目錄與基礎配置
  - 設置 axios baseURL 與攔截器
  - 實現 401 錯誤自動刷新 token 邏輯
  - 實現統一錯誤處理
  - **驗證：** API 呼叫正常，攔截器正確處理響應

## 2. 認證狀態管理

- [x] 2.1 創建 authStore
  - 定義狀態：isAuthenticated、user、accessToken、refreshToken、loading、error
  - 實現 actions：login、logout、refreshToken、initializeAuth
  - 實現 getters：isLoggedIn、userDisplayName
  - 配置 Pinia persist 插件（持久化到 localStorage）
  - **驗證：** Store 狀態正確保存與恢復

- [x] 2.2 創建認證 API 客戶端函數
  - 實現 login(email, password) 函數
  - 實現 oauthCallback(provider, code) 函數
  - 實現 logout() 函數
  - 實現 refreshToken() 函數
  - 添加錯誤處理與用戶提示
  - **驗證：** 各函數正確呼叫後端 API

## 3. 登入頁面與 OAuth 回調

- [ ] 3.1 創建登入頁面 (LoginView.vue)
  - 實現 Email+Password 登入表單
  - 表單驗證（Email 格式、密碼必填）
  - 呼叫 authStore.login
  - 處理登入成功/失敗狀態
  - 添加載入中狀態與錯誤提示
  - **驗證：** 輸入有效帳號可成功登入並重導向

- [ ] 3.2 添加 OAuth 登入按鈕
  - 在登入頁添加 Google 登入按鈕
  - 在登入頁添加 LINE 登入按鈕
  - 實現 OAuth 重導向邏輯
  - **驗證：** 點擊按鈕正確重導向至 OAuth 提供商

- [ ] 3.3 實現 OAuth 回調處理
  - 創建 AuthCallbackView.vue 頁面
  - 從 URL 取得 code 參數
  - 呼叫 authStore.oauthCallback
  - 處理回調成功/失敗邏輯
  - **驗證：** OAuth 授權後正確回導並完成登入

## 4. 收藏狀態管理

- [ ] 4.1 創建 favoritesStore
  - 定義狀態：favorites、isFavoritedMap、loading、error
  - 實現 actions：fetchFavorites、addFavorite、removeFavorite、checkFavorited
  - 實現 getters：favoritedSpots、favoriteCount、isFavorited
  - **驗證：** Store 狀態正確更新

- [ ] 4.2 創建收藏 API 客戶端函數
  - 實現 addFavorite(spotId) 函數
  - 實現 removeFavorite(spotId) 函數
  - 實現 getFavorites(page, pageSize) 函數
  - 實現 checkFavorited(spotId) 函數
  - 添加錯誤處理
  - **驗證：** 各函數正確呼叫後端 API

## 5. 收藏按鈕組件

- [ ] 5.1 實現 FavoriteButton.vue 組件
  - 顯示收藏狀態（已收藏/未收藏）
  - 處理點擊事件（添加/取消收藏）
  - 實現樂觀更新（立即更新 UI）
  - 實現錯誤回滾邏輯
  - 添加載入中狀態
  - **驗證：** 點擊按鈕正確添加/取消收藏

- [ ] 5.2 處理未登入狀態
  - 未登入時隱藏收藏按鈕或顯示「登入後收藏」
  - 點擊時引導至登入頁面
  - **驗證：** 未登入用戶看到適當提示

- [ ] 5.3 整合到景點詳情頁
  - 在 SpotDetailView.vue 中加入 FavoriteButton
  - 頁面載入時呼叫 checkFavorited
  - 登入後自動更新收藏狀態
  - **驗證：** 景點詳情頁正確顯示收藏狀態

## 6. 個人收藏列表頁面

- [ ] 6.1 創建 FavoritesView.vue 頁面
  - 呼叫 getFavorites 取得收藏列表
  - 顯示景點卡片列表（名稱、分類、縮圖、收藏時間）
  - 實現分頁控制
  - 處理空狀態（無收藏）
  - 添加載入中狀態
  - **驗證：** 收藏列表正確顯示並支持分頁

- [ ] 6.2 實現快速取消收藏
  - 在景點卡片添加「取消收藏」按鈕
  - 實現樂觀更新（立即移除卡片）
  - 實現錯誤回滾
  - **驗證：** 快速取消收藏功能正常

- [ ] 6.3 添加收藏列表入口
  - 在導航列添加「我的收藏」連結
  - 僅登入用戶顯示
  - **驗證：** 導航列正確顯示/隱藏連結

## 7. 用戶體驗優化

- [ ] 7.1 實現登出功能
  - 在導航列添加登出按鈕
  - 呼叫 authStore.logout
  - 清除本地狀態並重導向
  - **驗證：** 登出後無法訪問需認證頁面

- [ ] 7.2 優化錯誤提示
  - 統一錯誤提示樣式
  - 添加 Toast 或 Alert 組件
  - 處理網路錯誤提示
  - **驗證：** 錯誤訊息清晰易懂

- [ ] 7.3 優化載入狀態
  - 添加 Skeleton 或 Spinner 組件
  - 在 API 呼叫時顯示載入中
  - **驗證：** 載入狀態流暢不卡頓

## 8. 整合測試

- [ ] 8.1 手動測試認證流程
  - 測試 Email+Password 登入
  - 測試 OAuth 登入（Google、LINE）
  - 測試登出功能
  - 測試 Token 過期自動刷新
  - 測試路由守衛
  - **驗證：** 所有認證場景正常運作

- [ ] 8.2 手動測試收藏功能
  - 測試添加收藏
  - 測試取消收藏
  - 測試收藏列表分頁
  - 測試收藏狀態同步
  - 測試樂觀更新與錯誤回滾
  - **驗證：** 所有收藏場景正常運作

- [ ] 8.3 跨頁面狀態測試
  - 測試登入狀態持久化
  - 測試收藏狀態跨頁面同步
  - 測試登入前後狀態變化
  - **驗證：** 狀態管理正確無誤

## 9. 部署準備

- [ ] 9.1 配置環境變數
  - 設置後端 API URL
  - 配置 OAuth Client ID（Google、LINE）
  - **驗證：** 環境變數正確載入

- [ ] 9.2 建置生產版本
  - 執行 npm run build
  - 檢查建置輸出
  - **驗證：** 建置成功無錯誤
