using System.ComponentModel.DataAnnotations;

namespace Maplay.Auth;

/// <summary>使用者註冊請求</summary>
public record RegisterRequest(
    /// <summary>電子郵件地址</summary>
    [Required, EmailAddress] string Email,
    /// <summary>密碼 (至少 8 字元，需包含大小寫字母、數字、特殊字符)</summary>
    [Required] string Password,
    /// <summary>顯示名稱 (1-100 字元)</summary>
    [Required, StringLength(100, MinimumLength = 1)] string DisplayName);

/// <summary>使用者登入請求</summary>
public record LoginRequest(
    /// <summary>電子郵件地址</summary>
    [Required, EmailAddress] string Email,
    /// <summary>密碼</summary>
    [Required] string Password);

/// <summary>使用者資訊</summary>
public record UserDto(
    /// <summary>使用者 ID</summary>
    Guid Id,
    /// <summary>電子郵件地址</summary>
    string? Email,
    /// <summary>顯示名稱</summary>
    string DisplayName,
    /// <summary>頭像 URL</summary>
    string? AvatarUrl,
    /// <summary>角色 (User/Admin)</summary>
    string Role,
    /// <summary>認證提供者 (Email/Google/Line)</summary>
    string Provider);

/// <summary>認證結果</summary>
public record AuthResult(
    /// <summary>JWT 存取權杖</summary>
    string AccessToken,
    /// <summary>權杖過期時間 (秒)</summary>
    int ExpiresInSeconds,
    /// <summary>使用者資訊</summary>
    UserDto User);

/// <summary>OAuth 回調請求：以授權碼換取權杖（前端取得 code 後送來）</summary>
public record OAuthCallbackRequest(
    /// <summary>OAuth 授權碼</summary>
    [Required] string Code,
    /// <summary>重導向 URI</summary>
    string? RedirectUri);
