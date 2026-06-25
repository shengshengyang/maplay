namespace Maplay.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "user";
    public string Provider { get; set; } = "local";
    public string? ProviderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public static class UserRoles
{
    public const string User = "user";
    public const string Admin = "admin";
}

public static class AuthProviders
{
    public const string Local = "local";
    public const string Google = "google";
    public const string Line = "line";
}
