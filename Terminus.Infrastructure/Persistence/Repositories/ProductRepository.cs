using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Infrastructure.Persistence.Repositories;

public class ProductRepository(TerminusDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await dbContext.Products.AddAsync(product, cancellationToken);

    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Remove(product);
        return Task.CompletedTask;
    }
    
    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await dbContext.Products.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
}