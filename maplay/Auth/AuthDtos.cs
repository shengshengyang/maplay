using System.ComponentModel.DataAnnotations;

namespace Maplay.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required, StringLength(100, MinimumLength = 1)] string DisplayName);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserDto(
    Guid Id,
    string? Email,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    string Provider);

public record AuthResult(string AccessToken, int ExpiresInSeconds, UserDto User);

/// <summary>OAuth callback：以授權碼換 token（前端取得 code 後送來）。</summary>
public record OAuthCallbackRequest(
    [Required] string Code,
    string? RedirectUri);
