using Diaspora.Client.Api.Utils;
using Diaspora.Identity.Application.Authentication.Logout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diaspora.Client.Api.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class LogoutController(ISender sender) : ControllerBase
    {
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var rawToken = Request.Cookies[RefreshTokenCookie.Name];
            await sender.Send(new LogoutCommand(rawToken), cancellationToken);

            Response.Cookies.Delete(RefreshTokenCookie.Name, RefreshTokenCookie.DeleteOptions());

            return NoContent();
        }
    }
}
