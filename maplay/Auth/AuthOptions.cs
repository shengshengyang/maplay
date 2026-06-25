namespace Maplay.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "maplay";
    public string Audience { get; set; } = "maplay";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}

public class OAuthProviderOptions
{
    public GoogleOptions Google { get; set; } = new();
    public LineOptions Line { get; set; } = new();

    public class GoogleOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public bool Enabled => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
    }

    public class LineOptions
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelSecret { get; set; } = string.Empty;
        public bool Enabled => !string.IsNullOrWhiteSpace(ChannelId) && !string.IsNullOrWhiteSpace(ChannelSecret);
    }
}
