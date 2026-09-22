using Diaspora.Client.Api.Controllers.Auth.ViewModels;
using Diaspora.Identity.Application.Authentication.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Diaspora.Client.Api.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController(ISender sender) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await sender.Send(command, cancellationToken);

            if (result.Succeeded)
            {
                var response = new AuthResponse(
                    result.AccessToken,
                    result.ExpiresAtUtc,
                    new UserSummary(result.UserId, result.Email, result.FirstName, result.LastName));

                return Ok(response);
            }

            if (result.FieldErrors.Count > 0)
            {
                var modelState = new ModelStateDictionary();
                foreach (var (field, messages) in result.FieldErrors)
                {
                    foreach (var message in messages)
                    {
                        modelState.AddModelError(field, message);
                    }
                }

                return ValidationProblem(modelState);
            }

            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials",
                detail: "The email or password is incorrect.");
        }
    }
}
