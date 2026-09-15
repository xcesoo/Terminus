using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Infrastructure.Persistence;
using Terminus.Infrastructure.Persistence.Repositories;

namespace Terminus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TerminusDbContext>(o =>
        {
            o.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TerminusDbContext>());
        
        services.AddScoped<IWaybillRepository, WaybillRepository>();
        services.AddScoped<IConsumerRepository, ConsumerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        
        return services;
    }
}