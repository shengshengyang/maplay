# auth Specification

## Purpose
管理使用者身分認證與授權：帳密註冊/登入、JWT 存取權杖與 Refresh 權杖輪替、OAuth（Google / Line）登入與帳號合併，以及取得個人資料。

## Requirements

### Requirement: 會員註冊
系統 SHALL 提供帳密註冊端點 `POST /api/auth/register`，驗證 email 唯一性與密碼強度後建立 `role=user`、`provider=local` 的帳號，密碼 MUST 以雜湊儲存且不得明碼記錄於日誌。

#### Scenario: 成功註冊
- **WHEN** 提交未被使用的 email、長度 ≥ 8 且含英數字的密碼、長度 1–100 的 displayName
- **THEN** 系統建立帳號並回 201，回應不含密碼或雜湊

#### Scenario: email 重複
- **WHEN** 提交的 email 已存在
- **THEN** 系統回 409 並 `errorCode=EMAIL_ALREADY_EXISTS`

#### Scenario: 密碼強度不足
- **WHEN** 提交長度 < 8 或缺英文字母或缺數字的密碼
- **THEN** 系統回 400 `VALIDATION_ERROR`，並於 `data.errors.password` 說明原因

### Requirement: 帳密登入與權杖下發
系統 SHALL 提供 `POST /api/auth/login`，驗證通過後於回應 body 回傳效期 15 分鐘的 JWT accessToken，並以 `HttpOnly; Secure; SameSite=Strict` Cookie 下發效期 7 天的 refreshToken。帳號或密碼錯誤時 MUST 回相同錯誤而不洩漏是帳號或密碼錯誤。

#### Scenario: 登入成功
- **WHEN** 提交正確的 email 與密碼
- **THEN** 回 200，body 含 accessToken 與使用者基本資料，且 Set-Cookie 含 refreshToken

#### Scenario: 帳密錯誤
- **WHEN** 提交錯誤的 email 或密碼
- **THEN** 回 401 `INVALID_CREDENTIALS`，且訊息不指明是帳號或密碼錯誤

### Requirement: 權杖刷新與輪替
系統 SHALL 提供 `POST /api/auth/refresh`，自 Cookie 取 refreshToken 驗證後簽發新 accessToken 並輪替 refreshToken；無效或過期的 refreshToken MUST 回 401。

#### Scenario: 刷新成功
- **WHEN** 帶有效 refreshToken Cookie 呼叫刷新
- **THEN** 回 200 並下發新 accessToken 與新的 refreshToken Cookie

#### Scenario: refreshToken 失效
- **WHEN** refreshToken 不存在、過期或已被撤銷
- **THEN** 回 401 `REFRESH_INVALID`

### Requirement: 登出
系統 SHALL 提供 `POST /api/auth/logout`，撤銷後端 refreshToken 並清除 Cookie。

#### Scenario: 登出後權杖失效
- **WHEN** 使用者登出後，再以登出前的 refreshToken 呼叫刷新
- **THEN** 回 401 `REFRESH_INVALID`

### Requirement: 取得個人資料
系統 SHALL 提供 `GET /api/auth/me`，需 Bearer accessToken，回傳 `id, email, displayName, avatarUrl, role, provider`。

#### Scenario: 已認證取得資料
- **WHEN** 帶有效 accessToken 呼叫
- **THEN** 回 200 與目前使用者資料

#### Scenario: 未認證
- **WHEN** 未帶或帶過期 accessToken
- **THEN** 回 401 `UNAUTHORIZED` 或 `TOKEN_EXPIRED`

### Requirement: OAuth 登入與帳號合併
系統 SHALL 提供 Google（`/api/auth/google`）與 Line（`/api/auth/line`）OAuth 流程，授權成功後依識別建立或合併帳號，並回傳與帳密登入相同的權杖結構。當 OAuth email 已存在 local 帳號時 MUST 合併 provider 而非建立重複帳號；Line 可能無 email，系統 SHALL 以 providerId 作為唯一識別。

#### Scenario: 首次 Google 登入建立帳號
- **WHEN** 使用未註冊過的 Google 帳號完成授權
- **THEN** 系統建立 `provider=google`、`role=user` 帳號並回傳權杖

#### Scenario: Google email 已有 local 帳號
- **WHEN** Google 授權回傳的 email 已存在 local 帳號
- **THEN** 系統將 google provider 綁定至既有帳號，不建立重複帳號

#### Scenario: Line 無 email
- **WHEN** Line 授權未提供 email
- **THEN** 系統以 providerId 建立或比對帳號，不因缺 email 失敗
