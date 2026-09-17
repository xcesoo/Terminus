using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces;
using Terminus.Domain.ValueObjects;
using Terminus.Infrastructure.Configuration;

namespace Terminus.Infrastructure.Services;

public class DeliveryCalculatorService(DeliveryPricingOptions pricing) : IDeliveryCalculatorService
{
    public DeliveryCostBreakdown Calculate(TransportType transportType, int totalItemsQuantity, decimal declaredValue)
    {
        var transportMultiplier = transportType switch
        {
            TransportType.Auto => pricing.AutoMultiplier,
            TransportType.Train => pricing.TrainMultiplier,
            TransportType.Avia => pricing.AviaMultiplier,
            _ => throw new ArgumentOutOfRangeException(nameof(transportType), "Невідомий тип транспорту")
        };

        var baseDeliveryCost = totalItemsQuantity * pricing.BasePricePerItem;
        var commissionCost = declaredValue * pricing.CommissionRate;

        return new DeliveryCostBreakdown(baseDeliveryCost, commissionCost, transportMultiplier);
    }
}
