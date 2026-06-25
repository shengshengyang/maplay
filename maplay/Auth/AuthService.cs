using System.Text.RegularExpressions;
using Maplay.Common;
using Maplay.Data;
using Maplay.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maplay.Auth;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest req, CancellationToken ct);
    Task<(AuthResult result, string refreshRaw)> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<(AuthResult result, string refreshRaw)> RefreshAsync(string? refreshRaw, CancellationToken ct);
    Task LogoutAsync(string? refreshRaw, CancellationToken ct);
    Task<(AuthResult result, string refreshRaw)> OAuthLoginAsync(OAuthUserInfo info, CancellationToken ct);
    Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthService(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        ValidatePassword(req.Password);

        var email = req.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == email, ct);
        if (exists)
            throw AppException.Conflict(ErrorCodes.EmailAlreadyExists, "此 email 已被註冊");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            DisplayName = req.DisplayName.Trim(),
            Role = UserRoles.User,
            Provider = AuthProviders.Local,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<(AuthResult, string)> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // 不洩漏是帳號還是密碼錯誤
        if (user?.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw AppException.Unauthorized("帳號或密碼錯誤", ErrorCodes.InvalidCredentials);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<(AuthResult, string)> RefreshAsync(string? refreshRaw, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshRaw))
            throw AppException.Unauthorized("refreshToken 不存在", ErrorCodes.RefreshInvalid);

        var hash = _jwt.HashRefreshToken(refreshRaw);
        var token = await _db.RefreshTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || token.RevokedAt is not null || token.ExpiresAt <= DateTime.UtcNow || token.User is null)
            throw AppException.Unauthorized("refreshToken 無效或已過期", ErrorCodes.RefreshInvalid);

        // 輪替：撤銷舊 token 後簽發新組
        token.RevokedAt = DateTime.UtcNow;
        var result = await IssueTokensAsync(token.User, ct);
        return result;
    }

    public async Task LogoutAsync(string? refreshRaw, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshRaw)) return;
        var hash = _jwt.HashRefreshToken(refreshRaw);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);
        if (token is not null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<(AuthResult, string)> OAuthLoginAsync(OAuthUserInfo info, CancellationToken ct)
    {
        // 1) 先以 (provider, providerId) 比對
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Provider == info.Provider && u.ProviderId == info.ProviderId, ct);

        // 2) 若無，且有 email，嘗試合併既有帳號（不建立重複）
        if (user is null && !string.IsNullOrEmpty(info.Email))
        {
            var email = info.Email.ToLowerInvariant();
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (existing is not null)
            {
                existing.ProviderId ??= info.ProviderId; // 綁定 OAuth 識別至既有帳號
                if (string.IsNullOrEmpty(existing.AvatarUrl)) existing.AvatarUrl = info.AvatarUrl;
                existing.UpdatedAt = DateTime.UtcNow;
                user = existing;
            }
        }

        // 3) 仍無則建立新帳號（Line 可能無 email）
        if (user is null)
        {
            user = new User
            {
                Email = info.Email?.ToLowerInvariant(),
                DisplayName = info.DisplayName,
                AvatarUrl = info.AvatarUrl,
                Role = UserRoles.User,
                Provider = info.Provider,
                ProviderId = info.ProviderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw AppException.Unauthorized();
        return ToDto(user);
    }

    private async Task<(AuthResult, string)> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (access, expiresIn) = _jwt.CreateAccessToken(user);
        var (raw, hash, expiresAt) = _jwt.CreateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        return (new AuthResult(access, expiresIn, ToDto(user)), raw);
    }

    private static void ValidatePassword(string password)
    {
        var ok = password.Length >= 8
                 && Regex.IsMatch(password, "[A-Za-z]")
                 && Regex.IsMatch(password, "[0-9]");
        if (!ok)
            throw AppException.Validation("password", "密碼長度需 ≥ 8 且同時包含英文字母與數字");
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Email, u.DisplayName, u.AvatarUrl, u.Role, u.Provider);
}
