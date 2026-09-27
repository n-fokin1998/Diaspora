namespace Diaspora.Identity.Application.Common.Abstractions
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
