namespace Terminus.Domain.ValueObjects;

public record DeliveryCostBreakdown(decimal BaseCost, decimal CommissionCost, decimal TransportMultiplier)
{
    public decimal Total => (BaseCost + CommissionCost) * TransportMultiplier;
}
