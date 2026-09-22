using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace Diaspora.Identity.Infrastructure.Authentication;

internal class JwtTokenService(JwtOptions options) : IJwtTokenService
{
    public IssuedToken IssueAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(options.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new IssuedToken(accessToken, expiresAtUtc);
    }
}
