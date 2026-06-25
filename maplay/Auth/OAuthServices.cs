using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Maplay.Common;
using Maplay.Data.Entities;
using Microsoft.Extensions.Options;

namespace Maplay.Auth;

/// <summary>OAuth 授權後取得的標準化使用者資訊。</summary>
public record OAuthUserInfo(string Provider, string ProviderId, string? Email, string DisplayName, string? AvatarUrl);

public interface IOAuthService
{
    string Provider { get; }
    bool Enabled { get; }
    Task<OAuthUserInfo> ExchangeAsync(string code, string? redirectUri, CancellationToken ct);
}

/// <summary>Google OAuth：以 authorization code 換 token 並取得 userinfo。</summary>
public class GoogleOAuthService : IOAuthService
{
    private readonly OAuthProviderOptions.GoogleOptions _opt;
    private readonly IHttpClientFactory _http;

    public GoogleOAuthService(IOptions<OAuthProviderOptions> opt, IHttpClientFactory http)
    {
        _opt = opt.Value.Google;
        _http = http;
    }

    public string Provider => AuthProviders.Google;
    public bool Enabled => _opt.Enabled;

    public async Task<OAuthUserInfo> ExchangeAsync(string code, string? redirectUri, CancellationToken ct)
    {
        var client = _http.CreateClient();
        var tokenResp = await client.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _opt.ClientId,
                ["client_secret"] = _opt.ClientSecret,
                ["redirect_uri"] = redirectUri ?? "",
                ["grant_type"] = "authorization_code"
            }), ct);

        if (!tokenResp.IsSuccessStatusCode)
            throw AppException.Unauthorized("Google 授權失敗", ErrorCodes.OAuthFailed);

        var token = await tokenResp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var accessToken = token.GetProperty("access_token").GetString()!;

        var req = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var infoResp = await client.SendAsync(req, ct);
        if (!infoResp.IsSuccessStatusCode)
            throw AppException.Unauthorized("Google 使用者資訊取得失敗", ErrorCodes.OAuthFailed);

        var info = await infoResp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var sub = info.GetProperty("sub").GetString()!;
        var email = info.TryGetProperty("email", out var e) ? e.GetString() : null;
        var name = info.TryGetProperty("name", out var n) ? n.GetString() : null;
        var picture = info.TryGetProperty("picture", out var p) ? p.GetString() : null;

        return new OAuthUserInfo(Provider, sub, email, name ?? email ?? "Google 使用者", picture);
    }
}

/// <summary>Line OAuth：以 code 換 token，解析 id_token 取得 sub（可能無 email）。</summary>
public class LineOAuthService : IOAuthService
{
    private readonly OAuthProviderOptions.LineOptions _opt;
    private readonly IHttpClientFactory _http;

    public LineOAuthService(IOptions<OAuthProviderOptions> opt, IHttpClientFactory http)
    {
        _opt = opt.Value.Line;
        _http = http;
    }

    public string Provider => AuthProviders.Line;
    public bool Enabled => _opt.Enabled;

    public async Task<OAuthUserInfo> ExchangeAsync(string code, string? redirectUri, CancellationToken ct)
    {
        var client = _http.CreateClient();
        var tokenResp = await client.PostAsync("https://api.line.me/oauth2/v2.1/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri ?? "",
                ["client_id"] = _opt.ChannelId,
                ["client_secret"] = _opt.ChannelSecret
            }), ct);

        if (!tokenResp.IsSuccessStatusCode)
            throw AppException.Unauthorized("Line 授權失敗", ErrorCodes.OAuthFailed);

        var token = await tokenResp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var idToken = token.GetProperty("id_token").GetString()!;
        var payload = DecodeJwtPayload(idToken);

        var sub = payload.GetProperty("sub").GetString()!;
        var email = payload.TryGetProperty("email", out var e) ? e.GetString() : null; // Line 常無 email
        var name = payload.TryGetProperty("name", out var n) ? n.GetString() : null;
        var picture = payload.TryGetProperty("picture", out var p) ? p.GetString() : null;

        return new OAuthUserInfo(Provider, sub, email, name ?? "Line 使用者", picture);
    }

    private static JsonElement DecodeJwtPayload(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) throw AppException.Unauthorized("Line id_token 格式錯誤", ErrorCodes.OAuthFailed);
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}
