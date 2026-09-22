using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.Users;
using MediatR;

namespace Diaspora.Identity.Application.Authentication.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return RegisterResult.EmailConflict();
        }

        var hashed = passwordHasher.Hash(request.Password);
        var user = User.Register(
            request.Email,
            hashed.Hash,
            hashed.Salt,
            hashed.Iterations,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Location,
            DateTime.UtcNow);

        userRepository.AddUser(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var issuedToken = jwtTokenService.IssueAccessToken(user);

        return RegisterResult.Success(
            user.Id, user.Email, user.FirstName, user.LastName, issuedToken.AccessToken, issuedToken.ExpiresAtUtc);
    }
}
