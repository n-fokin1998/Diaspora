using MediatR;

namespace Diaspora.Identity.Application.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
