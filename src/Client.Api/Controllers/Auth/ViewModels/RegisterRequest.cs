namespace Diaspora.Client.Api.Controllers.Auth.ViewModels;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Location);
