# JWT Token 測試指南

本指南說明如何在 Swagger UI 中使用 JWT Token 進行 API 認證測試。

## 🔐 JWT 認證機制

親子資源地圖系統使用 JWT (JSON Web Token) 進行使用者認證：

- **Access Token**: 存取 API 的短期權杖 (15分鐘)
- **Refresh Token**: 刷新 Access Token 的長期權杖 (7天)
- **Bearer 認證**: HTTP 請求頭格式 `Authorization: Bearer {token}`

## 🚀 快速開始

### 1. 取得 JWT Token

#### 方法一：註冊新用戶

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "displayName": "測試用戶"
  }'
```

**回應範例：**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresInSeconds": 900,
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "test@example.com",
      "displayName": "測試用戶",
      "role": "User",
      "provider": "Email"
    }
  }
}
```

#### 方法二：登入現有用戶

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

#### 方法三：OAuth 登入 (Google/Line)

```bash
# Google OAuth
curl -X POST http://localhost:8080/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"code": "google_auth_code", "redirectUri": "http://localhost:3000/callback"}'

# Line OAuth
curl -X POST http://localhost:8080/api/auth/line \
  -H "Content-Type: application/json" \
  -d '{"code": "line_auth_code", "redirectUri": "http://localhost:3000/callback"}'
```

### 2. 在 Swagger UI 中設定 Token

#### 步驟一：開啟 Swagger UI

訪問：http://localhost:8080/swagger

#### 步驟二：設定 JWT Token

1. 點擊右上角的「🔑 設定 Token���按鈕
2. 在彈出視窗中貼上您的 JWT Token
3. **重要**：僅貼上 Token 本體，不要包含 `Bearer ` 前綴
4. 點擊確定後，頁面會自動重新載入

#### 步驟三：驗證 Token 設定

- 重新載入後，Token 會自動附加到所有需要認證的 API 請求
- 您可以測試 `/api/auth/me` 端點來驗證認證是否正常

### 3. 測試需要認證的端點

#### 測試當前用戶資訊

```bash
# 使用 Swagger UI 測試
1. 找到 GET /api/auth/me 端點
2. 點擊「Try it out」
3. 點擊「Execute」執行
4. 查看回應結果 (應返回當前用戶資訊)
```

#### 測試建立景點

```bash
# 使用 Swagger UI 測試
1. 找到 POST /api/spots 端點
2. 點擊「Try it out」
3. 填寫景點資訊：
   {
     "name": "測試公園",
     "category": "公園",
     "lat": 25.033,
     "lng": 121.565,
     "address": "台北市測試路123號"
   }
4. 點擊「Execute」執行
5. 查看回應結果 (應返回新建景點的 ID)
```

## 🔄 Token 刷新

當 Access Token 過期時，使用 Refresh Token 取得新的 Access Token：

```bash
# Refresh Cookie 會自動在登入時設定
curl -X POST http://localhost:8080/api/auth/refresh \
  --cookie "refreshToken=your_refresh_token_cookie"
```

## 🗑️ 清除 Token

若要清除已設定的 Token：

1. 點擊右上角的「🗑️ 清除 Token」按鈕
2. 頁面會自動重新載入，Token 將被移除

## ⚠️ 注意事項

### 安全性提醒

- ⚠️ **開發環境專用**：Swagger UI 僅在開發環境中啟用
- 🔒 **Token 安全**：不要在生產環境中分享或暴露您的 JWT Token
- ⏰ **Token 過期**：Access Token 有效期為 15 分鐘，過期需重新登入或刷新
- 🍪 **Refresh Token**：自動以 HTTP-only Cookie 形式儲存

### 常見問題

#### Q: Token 設定後仍返回 401 Unauthorized？

**A:** 請檢查：
1. Token 格式是否正確 (不應包含 `Bearer ` 前綴)
2. Token 是否已過期 (嘗試重新登入)
3. 是否重新載入頁面使設定生效

#### Q: 如何查看當前設定的 Token？

**A:** 
1. 開啟瀏覽器開發者工具 (F12)
2. 切換到 Console 分頁
3. 執行：`localStorage.getItem('swagger_jwt_token')`

#### Q: Token 在哪裡儲存？

**A:** JWT Token 儲存在瀏覽器的 localStorage 中，僅用於 Swagger UI 測試。實際應用程式中應使用更安全的儲存方式。

## 📚 相關資源

- **API 文檔**: http://localhost:8080/swagger
- **開發指南**: [README.md](./README.md)
- **API 詳細說明**: [OPENAPI_GUIDE.md](./OPENAPI_GUIDE.md)

---

**最後更新**: 2026-07-05
**適用版本**: v1.0.0