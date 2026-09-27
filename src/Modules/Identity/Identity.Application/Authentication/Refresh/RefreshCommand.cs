using MediatR;

namespace Diaspora.Identity.Application.Authentication.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<RefreshResult>;
