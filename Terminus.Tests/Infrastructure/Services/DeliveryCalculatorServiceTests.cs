using Terminus.Domain.Enums;
using Terminus.Infrastructure.Configuration;
using Terminus.Infrastructure.Services;

namespace Terminus.Tests.Infrastructure.Services;

public class DeliveryCalculatorServiceTests
{
    private static DeliveryPricingOptions DefaultOptions() => new()
    {
        BasePricePerItem = 100m,
        CommissionRate = 0.02m,
        AutoMultiplier = 1.0m,
        TrainMultiplier = 1.5m,
        AviaMultiplier = 3.0m
    };

    // Each transport type must pick its own configured multiplier from DeliveryPricingOptions.
    [Theory]
    [InlineData(TransportType.Auto, 1.0)]
    [InlineData(TransportType.Train, 1.5)]
    [InlineData(TransportType.Avia, 3.0)]
    public void Calculate_UsesCorrectMultiplierPerTransportType(TransportType type, decimal expectedMultiplier)
    {
        var service = new DeliveryCalculatorService(DefaultOptions());

        var result = service.Calculate(type, 10, 1000m);

        Assert.Equal(expectedMultiplier, result.TransportMultiplier);
    }

    // Base cost = quantity * BasePricePerItem, commission = declaredValue * CommissionRate, matching the documented formula.
    [Fact]
    public void Calculate_ComputesBaseAndCommissionCostsFromOptions()
    {
        var service = new DeliveryCalculatorService(DefaultOptions());

        var result = service.Calculate(TransportType.Auto, 48, 1089950m);

        Assert.Equal(4800m, result.BaseCost); // 48 * 100
        Assert.Equal(21799m, result.CommissionCost); // 1089950 * 0.02
        Assert.Equal(26599m, result.Total); // (4800 + 21799) * 1.0
    }

    // Zero quantity and zero declared value must produce a zero total, not an error.
    [Fact]
    public void Calculate_WithZeroQuantityAndValue_ReturnsZeroCost()
    {
        var service = new DeliveryCalculatorService(DefaultOptions());

        var result = service.Calculate(TransportType.Train, 0, 0m);

        Assert.Equal(0m, result.Total);
    }

    // An undefined enum value must fail loudly instead of silently picking a default multiplier.
    [Fact]
    public void Calculate_WithUnknownTransportType_Throws()
    {
        var service = new DeliveryCalculatorService(DefaultOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Calculate((TransportType)999, 1, 1m));
    }

    // Changing appsettings-backed options changes the resulting calculation — no hardcoded values left over.
    [Fact]
    public void Calculate_ReflectsCustomPricingOptions()
    {
        var options = new DeliveryPricingOptions
        {
            BasePricePerItem = 50m,
            CommissionRate = 0.005m,
            AutoMultiplier = 1.0m,
            TrainMultiplier = 1.2m,
            AviaMultiplier = 2.5m
        };
        var service = new DeliveryCalculatorService(options);

        var result = service.Calculate(TransportType.Train, 48, 1089950m);

        Assert.Equal(2400m, result.BaseCost); // 48 * 50
        Assert.Equal(5449.75m, result.CommissionCost); // 1089950 * 0.005
        Assert.Equal(1.2m, result.TransportMultiplier);
    }
}
