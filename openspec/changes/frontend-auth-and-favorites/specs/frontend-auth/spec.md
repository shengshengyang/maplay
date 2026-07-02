# frontend-auth Specification

## Purpose
實現前端用戶認證功能，讓用戶可以登入、登出，並支援 OAuth 登入（Google、Line）。

## ADDED Requirements

### Requirement: 用戶登入
系統 SHALL 提供登入頁面，��援 Email+Password 登入，成功後儲存 JWT token 並重導向至原請求頁面。

#### Scenario: 成功登入
- **GIVEN** 用戶在登入頁面輸入有效的 Email 與 Password
- **WHEN** 用戶點擊「登入」按鈕
- **THEN** 系統呼叫 POST /api/auth/login
- **AND** 成功後儲存 accessToken 與 refreshToken 到 localStorage
- **AND** 更新 authStore 狀態（isAuthenticated=true, user=用戶信息）
- **AND** 重導向至原請求頁面（或首頁）

#### Scenario: 登入失敗
- **GIVEN** 用戶輸入的 Email 或密碼無效
- **WHEN** 用戶點擊「登入」按鈕
- **THEN** 系統顯示錯誤提示（「Email 或密碼錯誤」）
- **AND** 不儲存任何 token
- **AND** 用戶保持在登入頁面

#### Scenario: 網路錯誤
- **GIVEN** API 呼叫失敗（網路斷線或伺服器錯誤）
- **WHEN** 用戶點擊「登入」按鈕
- **THEN** 系統顯示錯誤提示（「網路連線失敗，請稍後再試」）
- **AND** 用戶保持在登入頁面

### Requirement: OAuth 登入
系統 SHALL 支援 Google 與 Line OAuth 登入，重導向至 OAuth 提供商��回調後自動完成登入流程。

#### Scenario: 點擊 OAuth 登入按鈕
- **GIVEN** 用戶在登入頁面
- **WHEN** 用戶點擊「使用 Google 登入」或「使用 LINE 登入」
- **THEN** 系統重導向至 OAuth 提供商授權頁面

#### Scenario: OAuth 回調成功
- **GIVEN** 用戶在 OAuth 提供商完成授權
- **WHEN** 回導向至 /auth/callback/:provider?code=...
- **THEN** 系統呼叫後端交換 token
- **AND** 成功後儲存 accessToken 與 refreshToken
- **AND** 重導向至原請求頁面（或首頁）

#### Scenario: OAuth 回調失敗
- **GIVEN** OAuth 提供商拒絕授權或回調失敗
- **WHEN** 回導向至 /auth/callback/:provider?error=...
- **THEN** 系統顯示錯誤提示
- **AND** 重導向至登入頁面

### Requirement: 用戶登出
系統 SHALL 提供登出功能，清除本地 token 並重導向至首頁。

#### Scenario: 成功登出
- **GIVEN** 用戶已登入
- **WHEN** 用戶點擊「登出」按鈕
- **THEN** 系統清除 localStorage 中的 token
- **AND** 重置 authStore 狀態（isAuthenticated=false, user=null）
- **AND** 重導向至首頁

#### Scenario: 登出後訪問需認證頁面
- **GIVEN** 用戶已登出
- **WHEN** 用戶試圖訪問收藏列表頁面
- **THEN** 系統重導向至登入頁面
- **AND** 顯示提示「請先登入」

### Requirement: Token 自動刷新
系統 SHALL 在 accessToken 過期時自動使用 refreshToken 刷新，失敗則引導用戶重新登入。

#### Scenario: AccessToken 過期自動刷新
- **GIVEN** 用戶已登入，accessToken 過期但 refreshToken 有效
- **WHEN** API 呼叫返回 401 UNAUTHORIZED
- **THEN** 系統自動呼叫 POST /api/auth/refresh-token
- **AND** 成功後更新 localStorage 中的 token
- **AND** 重試原 API 呼叫

#### Scenario: RefreshToken 過期
- **GIVEN** 用戶已登入，refreshToken 過期
- **WHEN** API 呼叫返回 401 且刷新失敗
- **THEN** 系統清除所有 token
- **AND** 重導向至登入頁面
- **AND** 顯示提示「登入已過期，請重新登入」

### Requirement: 認證狀態檢查
系統 SHALL 提供路由守衛，保護需認證的頁面，未登入時重導向至登入頁。

#### Scenario: 訪問需認證頁面已登入
- **GIVEN** 用戶已登入
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統允許訪問，正常顯示頁面

#### Scenario: 訪問需認證頁面未登入
- **GIVEN** 用戶未登入
- **WHEN** 用戶訪問收藏列表頁面
- **THEN** 系統重導向至登入頁面
- **AND** 保存原請求路徑（登入後返回）
