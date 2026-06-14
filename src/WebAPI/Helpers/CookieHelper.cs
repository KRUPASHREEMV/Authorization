using Microsoft.AspNetCore.Http;

namespace TodoAuth.WebAPI.Helpers;

public static class CookieHelper
{
    private const string CookieName = "refreshToken";

    /// <summary>Sets the refresh token as an HttpOnly Secure SameSite=Strict cookie.</summary>
    public static void SetRefreshTokenCookie(HttpResponse response, string plainRefreshToken, bool isProduction)
    {
        response.Cookies.Append(CookieName, plainRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,           // Secure=true in production (HTTPS); false in dev (HTTP)
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(60),
            Path = "/api/auth"               // Scoped to auth endpoints only
        });
    }

    public static void ClearRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, new CookieOptions { Path = "/api/auth" });
    }
}
