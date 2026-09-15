namespace Terminus.Application.DTOs;

public record WaybillDto(
    string WaybillNumber,
    DateTime DispatchDate,
    string ProductName,
    int ShippedQuantity,
    string ConsumerName,
    string TransportType,
    string TransportDetails,
    decimal ServiceSum
);