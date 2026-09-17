using Identity.Domain.Users;

namespace Identity.Application.Common.Abstractions
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

        Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

        void AddUser(User user);
    }
}
