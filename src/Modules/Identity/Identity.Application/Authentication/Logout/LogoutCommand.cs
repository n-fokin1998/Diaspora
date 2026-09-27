using MediatR;

namespace Diaspora.Identity.Application.Authentication.Logout;

public sealed record LogoutCommand(string? RefreshToken) : IRequest<LogoutResult>;
