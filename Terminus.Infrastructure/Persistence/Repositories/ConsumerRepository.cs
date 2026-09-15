using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class ConsumerRepository(TerminusDbContext dbContext) : IConsumerRepository
{
    public Task<Consumer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Consumers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Consumer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Consumers.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Consumer consumer, CancellationToken cancellationToken = default) =>
        await dbContext.Consumers.AddAsync(consumer, cancellationToken);
    
    public async Task DeleteAsync(Consumer consumer, CancellationToken cancellationToken = default) =>
        await dbContext.Consumers.Where(c => c.Id == consumer.Id).ExecuteDeleteAsync(cancellationToken);
}