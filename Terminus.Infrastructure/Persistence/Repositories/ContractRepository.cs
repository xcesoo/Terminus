using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class ContractRepository(TerminusDbContext dbContext) : IContractRepository
{
    public Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Contracts
            .Include(c => c.Items)
            .ThenInclude(i=> i.Product)
            .Include(c => c.Waybills)
            .ThenInclude(w => w.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Contract>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product) 
            .ToListAsync(cancellationToken);
    
    public async Task<IReadOnlyCollection<Contract>> SearchByNumberAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Where(c => EF.Functions.ILike(c.ContractNumber, $"%{searchTerm}%") || 
                        EF.Functions.TrigramsAreSimilar(c.ContractNumber, searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts.AddAsync(contract, cancellationToken);

    public async Task DeleteAsync(Contract contract, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts.Where(c => c.Id == contract.Id).ExecuteDeleteAsync(cancellationToken);

    // Задача 1: Формування списку виробів і їхніх споживачів
    public async Task<IReadOnlyCollection<Contract>> GetContractsWithProductsAndConsumersAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Consumer)
            .Include(c => c.Items)          
            .ThenInclude(i => i.Product) 
            .ToListAsync(cancellationToken);

    // Задача 4: Сформувати список виробів для окремого споживача
    public async Task<IReadOnlyCollection<Contract>> GetContractsByConsumerIdWithProductsAsync(Guid consumerId, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.ConsumerId == consumerId)
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync(cancellationToken);
}