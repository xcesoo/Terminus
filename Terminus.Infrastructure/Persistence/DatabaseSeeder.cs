using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminus.Domain.Entities;

namespace Terminus.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private class ConsumerSeedDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
    }

    private class ProductSeedDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PriceListNumber { get; set; } = string.Empty;
    }

    private class SeedDataRoot
    {
        public List<ConsumerSeedDto>? Consumers { get; set; }
        public List<ProductSeedDto>? Products { get; set; }
    }
    
    public static async Task SeedAsync(TerminusDbContext context, ILogger logger)
    {
        await context.Database.MigrateAsync();
        
        var consumersExist = await context.Consumers.AnyAsync();
        var productsExist = await context.Products.AnyAsync();
        
        if (consumersExist && productsExist)
        {
            logger.LogInformation("DB has already been seeded.");
            return;
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "DbSeed.json");
        if (!File.Exists(filePath))
        {
            logger.LogError("File not found. Path: {Path}", filePath);
            return;
        }
        
        var seedData = await File.ReadAllTextAsync(filePath);
        var parsedData = JsonSerializer.Deserialize<SeedDataRoot>(seedData, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });      
        
        if (parsedData is null)
        {
            logger.LogError("Failed to deserialize seed data.");
            return;
        }
        
        if (!consumersExist && parsedData.Consumers is not null && parsedData.Consumers.Any())
        {
            var consumersToSeed = parsedData.Consumers.Select(dto => 
                Consumer.Create(dto.Name, dto.Address, dto.BankAccount)
            ).ToList();

            await context.Consumers.AddRangeAsync(consumersToSeed);
            logger.LogInformation("Seeded {Count} consumers.", consumersToSeed.Count);
        }
        
        if (!productsExist && parsedData.Products is not null && parsedData.Products.Any())
        {
            var productsToSeed = parsedData.Products.Select(dto => 
                Product.Create(dto.Code, dto.Name, dto.Price, dto.PriceListNumber)
            ).ToList();

            await context.Products.AddRangeAsync(productsToSeed);
            logger.LogInformation("Seeded {Count} products.", productsToSeed.Count);
        }
        
        await context.SaveChangesAsync();
    }
}