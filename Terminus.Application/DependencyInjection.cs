using Microsoft.Extensions.DependencyInjection;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;

namespace Terminus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        services.AddScoped<IRuleEngine, RuleEngine>();
        
        //auto add rules
        var applicationAssembly = typeof(RuleEngine).Assembly;
        var ruleTypes = applicationAssembly.GetTypes()
            .Where(t => typeof(IBusinessRule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var ruleType in ruleTypes)
        {
            services.AddScoped(typeof(IBusinessRule), ruleType);
        }
        
        return services;
    }
}