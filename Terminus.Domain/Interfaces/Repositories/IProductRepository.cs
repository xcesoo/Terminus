using Terminus.Domain.Entities;

namespace Terminus.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    //base crud operations
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByCodeAsync(string requestCode, CancellationToken cancellationToken);
}