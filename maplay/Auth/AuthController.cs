using Maplay.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maplay.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookie = "refreshToken";

    private readonly IAuthService _auth;
    private readonly ICurrentUser _current;
    private readonly IEnumerable<IOAuthService> _oauth;
    private readonly JwtOptions _jwtOpt;

    public AuthController(
        IAuthService auth,
        ICurrentUser current,
        IEnumerable<IOAuthService> oauth,
        Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOpt)
    {
        _auth = auth;
        _current = current;
        _oauth = oauth;
        _jwtOpt = jwtOpt.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var user = await _auth.RegisterAsync(req, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (result, refreshRaw) = await _auth.LoginAsync(req, ct);
        SetRefreshCookie(refreshRaw);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var raw = Request.Cookies[RefreshCookie];
        var (result, refreshRaw) = await _auth.RefreshAsync(raw, ct);
        SetRefreshCookie(refreshRaw);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var raw = Request.Cookies[RefreshCookie];
        await _auth.LogoutAsync(raw, ct);
        ClearRefreshCookie();
        return Ok(ApiResponse.Ok(new { message = "已登出" }));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _auth.GetMeAsync(_current.RequireUserId(), ct);
        return Ok(ApiResponse.Ok(user));
    }

    [HttpPost("google")]
    public Task<IActionResult> Google([FromBody] OAuthCallbackRequest req, CancellationToken ct)
        => OAuthLogin("google", req, ct);

    [HttpPost("line")]
    public Task<IActionResult> Line([FromBody] OAuthCallbackRequest req, CancellationToken ct)
        => OAuthLogin("line", req, ct);

    private async Task<IActionResult> OAuthLogin(string provider, OAuthCallbackRequest req, CancellationToken ct)
    {
        var svc = _oauth.FirstOrDefault(s => s.Provider == provider);
        if (svc is null || !svc.Enabled)
            throw new AppException(StatusCodes.Status501NotImplemented, ErrorCodes.NotImplemented,
                $"{provider} OAuth 未設定");

        var info = await svc.ExchangeAsync(req.Code, req.RedirectUri, ct);
        var (result, refreshRaw) = await _auth.OAuthLoginAsync(info, ct);
        SetRefreshCookie(refreshRaw);
        return Ok(ApiResponse.Ok(result));
    }

    private void SetRefreshCookie(string raw)
    {
        Response.Cookies.Append(RefreshCookie, raw, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtOpt.RefreshTokenDays)
        });
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Append(RefreshCookie, "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }
}
