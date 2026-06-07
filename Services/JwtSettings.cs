namespace HomeBridge.Services;

// Bound from the "Jwt" section of appsettings.json.
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HomeBridge.Api";
    public string Audience { get; set; } = "HomeBridge.Client";
    public int ExpiryHours { get; set; } = 8;
}
