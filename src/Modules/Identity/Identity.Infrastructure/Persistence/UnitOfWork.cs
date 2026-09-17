using Identity.Application.Common.Abstractions;

namespace Identity.Infrastructure.Persistence
{
    internal class UnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
    {
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
