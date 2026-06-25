# admin-moderation Specification

## Purpose
提供管理員審查使用者回報的景點（核准/退回）、編輯與軟刪除任意景點，以及管理使用者。所有端點僅限 admin 角色。

## Requirements

### Requirement: 管理端點授權
系統 SHALL 對所有 `/api/admin/**` 端點於後端強制 `admin` 角色驗證；非 admin MUST 回 403，未認證 MUST 回 401。前端路由守衛不得作為唯一防線。

#### Scenario: 一般使用者存取管理端點
- **WHEN** 以 `role=user` 的權杖呼叫任一 `/api/admin/**`
- **THEN** 回 403 `FORBIDDEN`

#### Scenario: 未認證存取管理端點
- **WHEN** 未帶權杖呼叫任一 `/api/admin/**`
- **THEN** 回 401 `UNAUTHORIZED`

### Requirement: 待審查清單
系統 SHALL 提供 `GET /api/admin/spots/pending`（分頁），回傳 `status=pending` 且未軟刪除的景點，含回報者、回報時間、分類、座標。

#### Scenario: 取得待審查清單
- **WHEN** admin 呼叫且存在 pending 景點
- **THEN** 回 200，分頁回傳 pending 景點

#### Scenario: 已軟刪除不出現於待審查
- **GIVEN** 一筆 pending 但已軟刪除的景點
- **WHEN** admin 取待審查清單
- **THEN** 結果不含該景點

### Requirement: 核准景點
系統 SHALL 提供 `PUT /api/admin/spots/{id}/approve`，將景點 `status` 改為 approved 並記錄 `reviewed_by`、`reviewed_at`。核准後 MUST 立即可被公開查詢取得。

#### Scenario: 核准後公開可見
- **GIVEN** 一筆 pending 景點
- **WHEN** admin 核准
- **THEN** `status=approved`，且立即出現在 `GET /api/spots/nearby`

### Requirement: 退回景點
系統 SHALL 提供 `PUT /api/admin/spots/{id}/reject`，rejectReason MUST 必填，將 `status` 改為 rejected 並記錄退回理由與審查者。

#### Scenario: 退回並記錄理由
- **WHEN** admin 提交含 rejectReason 的退回請求
- **THEN** `status=rejected`，`reject_reason` 與 `reviewed_by` 被記錄

#### Scenario: 退回未附理由
- **WHEN** admin 退回但未提供 rejectReason
- **THEN** 回 400 `VALIDATION_ERROR`

### Requirement: 編輯景點
系統 SHALL 提供 `PUT /api/admin/spots/{id}`，允許 admin 修改任意景點（含已核准）之所有可編輯欄位。

#### Scenario: 編輯已核准景點
- **WHEN** admin 修改已核准景點的名稱或設施
- **THEN** 回 200 並更新，`updated_at` 隨之變更

### Requirement: 景點軟刪除
系統 SHALL 提供 `DELETE /api/admin/spots/{id}`，採**軟刪除**：設定 `deleted_at` 時間戳記而不實際移除資料列，保留其評價與稽核資訊。已軟刪除景點 MUST 自所有公開查詢、詳情與待審查清單排除。系統 SHALL 提供 `POST /api/admin/spots/{id}/restore` 還原軟刪除景點。

#### Scenario: 軟刪除後自查詢排除
- **WHEN** admin 軟刪除某景點
- **THEN** 該景點 `deleted_at` 被設定，且不再出現於 nearby / detail / pending

#### Scenario: 評價保留
- **GIVEN** 一景點有評價
- **WHEN** admin 軟刪除該景點
- **THEN** 評價資料列仍保留於資料庫，未被實體刪除

#### Scenario: 還原軟刪除景點
- **WHEN** admin 對已軟刪除景點呼叫 restore
- **THEN** `deleted_at` 清空，景點依其 `status` 回復原有可見性

### Requirement: 使用者管理
系統 SHALL 提供 `GET /api/admin/users`（分頁、可依 email/displayName 搜尋），列出使用者及其角色與來源。

#### Scenario: 搜尋使用者
- **WHEN** admin 帶關鍵字查詢使用者
- **THEN** 回傳符合的使用者分頁清單，含 role 與 provider
