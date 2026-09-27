namespace Diaspora.Client.Api.Utils;

internal static class RefreshTokenCookie
{
    public const string Name = "refreshToken";
    private const string CookiePath = "/api/auth";

    public static CookieOptions Options(DateTime expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expiresAtUtc,
    };

    public static CookieOptions DeleteOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
    };
}
