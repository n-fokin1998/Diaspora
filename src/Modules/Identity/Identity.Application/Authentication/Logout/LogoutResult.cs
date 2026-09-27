namespace Diaspora.Identity.Application.Authentication.Logout;

public sealed class LogoutResult
{
    private LogoutResult()
    {
    }

    public static LogoutResult Success() => new();
}
