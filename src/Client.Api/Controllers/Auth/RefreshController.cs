using Diaspora.Client.Api.Controllers.Auth.ViewModels;
using Diaspora.Client.Api.Utils;
using Diaspora.Identity.Application.Authentication.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diaspora.Client.Api.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class RefreshController(ISender sender) : ControllerBase
    {
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var rawToken = Request.Cookies[RefreshTokenCookie.Name];
            if (string.IsNullOrEmpty(rawToken))
            {
                return SessionExpired();
            }

            var result = await sender.Send(new RefreshCommand(rawToken), cancellationToken);

            if (!result.Succeeded)
            {
                Response.Cookies.Delete(RefreshTokenCookie.Name, RefreshTokenCookie.DeleteOptions());
                return SessionExpired();
            }

            Response.Cookies.Append(RefreshTokenCookie.Name, result.RefreshToken, RefreshTokenCookie.Options(result.RefreshTokenExpiresAtUtc));

            var response = new AuthResponse(
                result.AccessToken,
                result.ExpiresAtUtc,
                new UserSummary(result.UserId, result.Email, result.FirstName, result.LastName));

            return Ok(response);
        }

        private ObjectResult SessionExpired() => Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Session expired",
            detail: "Please log in again.");
    }
}
