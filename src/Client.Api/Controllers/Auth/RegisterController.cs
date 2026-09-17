using Client.Api.Controllers.Auth.ViewModels;
using Identity.Application.Authentication.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Client.Api.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class RegisterController(ISender sender) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Location);

            var result = await sender.Send(command, cancellationToken);

            if (result.EmailAlreadyInUse)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Email already in use",
                    detail: "An account with this email address already exists.");
            }

            if (!result.Succeeded)
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

            var response = new AuthResponse(
                result.AccessToken,
                result.ExpiresAtUtc,
                new UserSummary(result.UserId, result.Email, result.FirstName, result.LastName));

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
