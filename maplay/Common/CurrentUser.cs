using System.Security.Claims;

namespace Maplay.Common;

/// <summary>從 HttpContext 取出目前認證使用者資訊。</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Role { get; }
    bool IsAdmin { get; }
    Guid RequireUserId();
}

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var sub = _principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? _principal?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Role => _principal?.FindFirstValue(ClaimTypes.Role) ?? _principal?.FindFirstValue("role");

    public bool IsAdmin => string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);

    public Guid RequireUserId() => UserId ?? throw AppException.Unauthorized();
}
