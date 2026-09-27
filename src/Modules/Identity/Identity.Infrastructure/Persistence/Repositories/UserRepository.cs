using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Diaspora.Identity.Infrastructure.Persistence.Repositories
{
    internal class UserRepository(IdentityDbContext dbContext) : IUserRepository
    {
        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            dbContext.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        public Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
            dbContext.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

        public void AddUser(User user) => dbContext.Users.Add(user);
    }
}
