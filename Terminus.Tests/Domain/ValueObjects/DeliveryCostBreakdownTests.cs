using Terminus.Domain.ValueObjects;

namespace Terminus.Tests.Domain.ValueObjects;

public class DeliveryCostBreakdownTests
{
    // Total is always (BaseCost + CommissionCost) * TransportMultiplier.
    [Theory]
    [InlineData(100, 10, 1, 110)]
    [InlineData(4800, 21799, 1.5, 39898.5)]
    [InlineData(0, 0, 3, 0)]
    public void Total_ComputesBaseCostPlusCommissionTimesMultiplier(decimal baseCost, decimal commissionCost, decimal multiplier, decimal expected)
    {
        var breakdown = new DeliveryCostBreakdown(baseCost, commissionCost, multiplier);

        Assert.Equal(expected, breakdown.Total);
    }

    // Being a record, two breakdowns with the same component values must compare equal.
    [Fact]
    public void Equality_IsValueBased()
    {
        var a = new DeliveryCostBreakdown(100m, 10m, 1.5m);
        var b = new DeliveryCostBreakdown(100m, 10m, 1.5m);

        Assert.Equal(a, b);
    }
}
