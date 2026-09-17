using Terminus.Domain.Enums;
using Terminus.Domain.ValueObjects;

namespace Terminus.Domain.Interfaces;

public interface IDeliveryCalculatorService
{
    DeliveryCostBreakdown Calculate(TransportType transportType, int totalItemsQuantity, decimal declaredValue);
}