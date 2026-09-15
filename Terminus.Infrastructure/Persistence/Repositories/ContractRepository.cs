using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class ContractRepository(TerminusDbContext dbContext) : IContractRepository
{
    public Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Contracts.Include(c => c.Waybills).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Contract>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Contracts.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts.AddAsync(contract, cancellationToken);

    public async Task DeleteAsync(Contract contract, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts.Where(c => c.Id == contract.Id).ExecuteDeleteAsync(cancellationToken);

    // Задача 1: Формування списку виробів і їхніх споживачів
    public async Task<IReadOnlyCollection<Contract>> GetContractsWithProductsAndConsumersAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Product)
            .Include(c => c.Consumer)
            .ToListAsync(cancellationToken);

    // Задача 4: Сформувати список виробів для окремого споживача
    public async Task<IReadOnlyCollection<Contract>> GetContractsByConsumerIdWithProductsAsync(Guid consumerId, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.ConsumerId == consumerId)
            .Include(c => c.Product)
            .ToListAsync(cancellationToken);
}