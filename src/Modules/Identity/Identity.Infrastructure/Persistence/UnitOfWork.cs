using Diaspora.Identity.Application.Common.Abstractions;

namespace Diaspora.Identity.Infrastructure.Persistence
{
    internal class UnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
    {
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
