using Terminus.Domain.Entities;

namespace Terminus.Domain.Interfaces.Repositories;

public interface IConsumerRepository
{
    //base crud operations
    Task<Consumer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Consumer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Consumer consumer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Consumer consumer, CancellationToken cancellationToken = default);
}