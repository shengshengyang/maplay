## 1. 資料庫遷移

- [ ] 1.1 創建 user_favorites 表 Flyway migration
  - 編寫 SQL 腳本：創建 user_favorites 表（id、user_id、spot_id、created_at）
  - 添加複合唯一索引 (user_id, spot_id)
  - 添加雙向索引 (idx_user_favorites_user_id、idx_user_favorites_spot_id)
  - 設置級聯刪除約束 (ON DELETE CASCADE)
  - **驗證：** `flyway migrate` 成功執行，表結構正確建立，索引有效

- [ ] 1.2 執行資料庫遷移並驗證
  - 執行 Flyway migration 到測試資料庫
  - 驗證 user_favorites 表存在且結構正確
  - 測試級聯刪除約束（刪除測試用戶和景點，確認收藏記錄自動清理）
  - **驗證：** 資料庫表查詢���常，約束功能運作符合預期

## 2. 後端資料模型與服務層

- [ ] 2.1 創建 UserFavorite 實體類與 DTO
  - 創建 `UserFavorite` 實體類（對應 user_favorites 表）
  - 創建相關 DTO（FavoriteDto、CreateFavoriteRequest、FavoriteListResponse）
  - 設置 EF Core 映射關係和約束
  - **驗證：** 編譯通過，屬性映射正確

- [ ] 2.2 實現 IFavoritesService 介面與實作
  - 創建 `IFavoritesService` 介面（AddFavoriteAsync、RemoveFavoriteAsync、GetUserFavoritesAsync、CheckFavoritedAsync）
  - 實現 `FavoritesService` 類，包含所有收藏 CRUD 邏輯
  - 實現重複收藏檢查和景點存在性驗證
  - 添加分頁支持和排序邏輯
  - **驗證：** 服務層邏輯覆蓋所有規格需求

- [ ] 2.3 註冊服務到 DI 容器
  - 在 Program.cs 中註冊 `IFavoritesService` 和 `FavoritesService`
  - 設置服務生命週期為 Scoped
  - **驗證：** 應用啟動正常，服務註冊無錯誤

## 3. 後端 API 控制器與權限調整

- [ ] 3.1 創建 FavoritesController
  - 創建 `FavoritesController` 並設置路由 `/api/favorites`
  - 實現 `POST /api/favorites`（添加收藏）
  - 實現 `DELETE /api/favorites/{spotId}`（取消收藏）
  - 實現 `GET /api/favorites`（個人收藏列表，支持分頁）
  - 實現 `GET /api/favorites/check/{spotId}`（檢查收藏狀態）
  - 添加適當的錯誤處理和響應狀態碼
  - **驗證：** 所有端點路由正確，基本 CRUD 功能運作

- [ ] 3.2 設置收藏相關端點權限
  - 為所有收藏端點添加 `[Authorize]` 標記
  - 確保未登入用戶返回 401 UNAUTHORIZED
  - **驗證：** 未登入訪問收藏端點正確返回 401

- [ ] 3.3 移除景點查詢端點的認證要求
  - 修改 `SpotsController`，移除查詢端點的 `[Authorize]` 標記
  - 保留操作端點（POST、PUT、DELETE）的認證要求
  - 確保 `GET /api/spots/nearby`、`GET /api/spots/{id}`、`GET /api/spots/active-temp` 允許匿名訪問
  - **驗證：** 匿名用戶可訪問景點查詢端點，操作端點仍需認證

- [ ] 3.4 更新 OpenAPI 文檔
  - 為新的收藏端點添加 XML 文檔註釋
  - 更新景點端點的安全定義（標記為公開訪問）
  - 重新生成 OpenAPI 文檔
  - **驗證：** Swagger UI 正確顯示所有端點和權限要求

## 4. 前端狀態管理與組件

- [ ] 4.1 創建 Pinia favorites store
  - 創建 `useFavoritesStore`，管理收藏狀態
  - 實現狀態：favorites、isFavorited、loading、error
  - 實現 actions：fetchFavorites、addFavorite、removeFavorite、checkFavorited
  - 實現 getters：favoritedSpots、favoriteCount
  - **驗證：** Store 狀態管理邏輯正確，actions 和 getters 工作正常

- [ ] 4.2 創建收藏相關 API 客戶端函數
  - 創建 API 函數：addFavorite、removeFavorite、getFavorites、checkFavorited
  - 處理認證 token 注入
  - 添加錯誤處理和用戶提示
  - **驗證：** API 調���正確，錯誤處理完善

- [ ] 4.3 實現收藏按鈕組件
  - 創建 `FavoriteButton.vue` 組件
  - 顯示收藏狀態（已收藏/未收藏）
  - 處理點擊事件（添加/取消收藏）
  - 處理登入狀態（未登入時隱藏或引導登入）
  - 添加加載狀態和錯誤提示
  - **驗證：** 組件交互正常，狀態切換流暢

- [ ] 4.4 實現個人收藏列表頁面
  - 創建 `FavoritesView.vue` 頁面
  - 顯示用戶收藏的景點列表（包含景點基本信息）
  - 支持分���和排序（按收藏時間）
  - 提供快速取消收藏功能
  - 處理空狀態和錯誤狀態
  - **驗證���** 頁面功能完整，用戶體驗良好

- [ ] 4.5 調整景點相關頁面的認證守衛
  - 修改路由守衛，允許未登入用戶訪問景點列表、詳情頁面
  - 在景點詳情頁集成收藏按鈕
  - 處理登入前後的狀態同步
  - **驗證：** 未登入用戶可正常瀏覽景點，登入後顯示收藏功能

## 5. 整合測試與優化

- [ ] 5.1 編寫後端單元測試
  - 測試 FavoritesService 的所有方法
  - 測試權限控制（匿名訪問、認證訪問）
  - 測試錯誤處理（重複收藏、景點不存在等）
  - 測試級聯刪除邏輯
  - **驗證：** 測試覆蓋率 ≥ 80%，所有測試通過

- [ ] 5.2 編寫後端集成測試
  - 測試完整的收藏流程（添加→查詢→刪除）
  - 測試分頁和排序功能
  - 測試並發收藏請求處理
  - 測試景點查詢的匿名訪問
  - **驗證：** 端到端流程測試通過，API 運作穩定

- [ ] 5.3 進行前端手動測試
  - 測試未登入狀態下的景點瀏覽
  - 測試登入後的收藏功能
  - 測試登入前後狀態同步
  - 測試錯誤處理和用戶提示
  - 測試收藏列表的分頁和排序
  - **驗證：** 用戶體驗流暢，無明顯缺陷

- [ ] 5.4 性能測試與優化
  - 測試收藏列表查詢性能（大量數據情況）
  - 測試並發收藏操作性能
  - 優化資料庫查詢索引（如需要）
  - 監控 API 響應時間
  - **驗證：** API 響應時間 < 500ms，並發處理穩定

## 6. 部署準備與驗證

- [ ] 6.1 準備部署腳本和文檔
  - 更新 Docker Compose 配置（如需要）
  - 準備資料庫遷移說明
  - 更新部署文檔和回滾計劃
  - **驗證：** 部署文檔完整，步驟清晰

- [ ] 6.2 進行預生產環境測試
  - 在測試環境執行完整部署流程
  - 驗證所有功能正常工作
  - 檢查數據完整性和安全性
  - 監控系統性能和錯誤日誌
  - **驗證：** 測試環境運行穩定，無阻塞性問題

- [ ] 6.3 執行生產環境部署
  - 執行資料庫遷移
  - 部署後端 API 服務
  - 部署前端應用
  - 執行煙霧測試驗證部署成功
  - 監控生產環境指標
  - **驗證：** 生產環境部署成功，功能正常，性能穩定
