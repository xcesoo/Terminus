using Terminus.Domain.Enums;

namespace Terminus.Domain.Interfaces;

public interface IDeliveryCalculatorService
{
    decimal Calculate(TransportType transportType, int totalItemsQuantity, decimal declaredValue);
}