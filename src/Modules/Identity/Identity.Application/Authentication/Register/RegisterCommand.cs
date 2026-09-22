using MediatR;

namespace Diaspora.Identity.Application.Authentication.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Location) : IRequest<RegisterResult>;
