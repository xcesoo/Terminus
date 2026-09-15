using Microsoft.EntityFrameworkCore;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Infrastructure.Persistence.Configurations;

namespace Terminus.Infrastructure.Persistence;

public class TerminusDbContext : DbContext, IUnitOfWork
{
    public TerminusDbContext(DbContextOptions<TerminusDbContext> options) : base(options) { }

    public DbSet<Consumer> Consumers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Waybill> Waybills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsumerConfiguration).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}