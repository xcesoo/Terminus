using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminus.Infrastructure.Persistence;

namespace Terminus.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TerminusDbContext>>();

        try
        {
            var context = services.GetRequiredService<TerminusDbContext>();
            await DatabaseSeeder.SeedAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}