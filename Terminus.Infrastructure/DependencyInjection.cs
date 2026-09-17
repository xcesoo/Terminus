using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Interfaces;
using Terminus.Domain.Interfaces.Repositories;
using Terminus.Infrastructure.Configuration;
using Terminus.Infrastructure.Pdf;
using Terminus.Infrastructure.Persistence;
using Terminus.Infrastructure.Persistence.Repositories;
using Terminus.Infrastructure.Services;

namespace Terminus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TerminusDbContext>(o =>
        {
            o.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        QuestPDF.Settings.License = LicenseType.Community;
        
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TerminusDbContext>());
        
        services.AddScoped<IWaybillRepository, WaybillRepository>();
        services.AddScoped<IConsumerRepository, ConsumerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        
        services.AddTransient<IPaymentDemandPdfGenerator, QuestPdfPaymentDemandGenerator>();
        services.AddTransient<IShipmentStatementPdfGenerator, QuestPdfShipmentStatementGenerator>();
        services.AddTransient<IContractPdfGenerator, QuestPdfContractGenerator>();

        var deliveryPricing = configuration.GetSection(DeliveryPricingOptions.SectionName).Get<DeliveryPricingOptions>()
                               ?? new DeliveryPricingOptions();
        services.AddSingleton(deliveryPricing);
        services.AddSingleton<IDeliveryCalculatorService, DeliveryCalculatorService>();

        return services;
    }
}