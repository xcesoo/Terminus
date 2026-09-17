using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terminus.Application.Exceptions;
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
        modelBuilder.HasPostgresExtension("pg_trgm");
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (GetPostgresException(ex) is { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateValueException("Значення, що вводиться, вже використовується.");
        }
        catch (Exception ex) when (GetPostgresException(ex) is { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new EntityInUseException("Неможливо видалити запис: він використовується в інших даних (договорах, накладних тощо).");
        }
    }

    private static PostgresException? GetPostgresException(Exception ex) =>
        ex as PostgresException ?? ex.InnerException as PostgresException;
}
