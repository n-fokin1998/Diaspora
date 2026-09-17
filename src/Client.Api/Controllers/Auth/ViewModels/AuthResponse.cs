namespace Client.Api.Controllers.Auth.ViewModels;

public sealed record UserSummary(Guid Id, string Email, string FirstName, string LastName);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserSummary User);
