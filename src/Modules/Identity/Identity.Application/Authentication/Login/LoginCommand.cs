using MediatR;

namespace Identity.Application.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
