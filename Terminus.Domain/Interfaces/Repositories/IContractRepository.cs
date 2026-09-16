using Terminus.Domain.Entities;

namespace Terminus.Domain.Interfaces.Repositories;

public interface IContractRepository
{
    //base crud operations
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Contract>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Contract>> SearchByNumberAsync(string searchTerm,
        CancellationToken cancellationToken = default);

    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task DeleteAsync(Contract contract, CancellationToken cancellationToken = default);

    // Задача 1: Формування списку виробів і їхніх споживачів.
    // Витягує всі договори разом із підключеними виробами та споживачами.
    Task<IReadOnlyCollection<Contract>> GetContractsWithProductsAndConsumersAsync(CancellationToken cancellationToken = default);

    // Задача 4: Сформувати список виробів для окремого споживача.
    // Витягує договори конкретного споживача разом із виробами.
    Task<IReadOnlyCollection<Contract>> GetContractsByConsumerIdWithProductsAsync(Guid consumerId, CancellationToken cancellationToken = default);
}