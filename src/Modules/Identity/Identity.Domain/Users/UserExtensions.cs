namespace Identity.Domain.Users
{
    public static class UserExtensions
    {
        public static string NormalizeEmail(this string email)
        {
            return email.Trim().ToUpperInvariant();
        }
    }
}
