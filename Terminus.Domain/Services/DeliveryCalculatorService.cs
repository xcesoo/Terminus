using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces;

namespace Terminus.Domain.Services;

public class DeliveryCalculatorService : IDeliveryCalculatorService
{
    public decimal Calculate(TransportType transportType, int totalItemsQuantity, decimal declaredValue)
    {
        const decimal BasePricePerItem = 100m;
        const decimal CommissionRate = 0.02m; 

        var transportMultiplier = transportType switch
        {
            TransportType.Auto => 1.0m,   
            TransportType.Train => 1.5m,
            TransportType.Avia => 3.0m, 
            _ => throw new ArgumentOutOfRangeException(nameof(transportType))
        };

        // Твоя формула: (комісія + ціна * кількість) * коефіцієнт
        var baseDeliveryCost = totalItemsQuantity * BasePricePerItem;
        var commissionCost = declaredValue * CommissionRate;

        return (baseDeliveryCost + commissionCost) * transportMultiplier;
    }
}