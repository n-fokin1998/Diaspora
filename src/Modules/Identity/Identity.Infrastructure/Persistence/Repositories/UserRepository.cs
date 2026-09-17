using Identity.Application.Common.Abstractions;
using Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories
{
    internal class UserRepository(IdentityDbContext dbContext) : IUserRepository
    {
        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            dbContext.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            dbContext.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        public void AddUser(User user) => dbContext.Users.Add(user);
    }
}
