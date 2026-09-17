using Terminus.Domain.Enums;

namespace Terminus.Application.DTOs;

public record WaybillDto(
    Guid Id,
    string WaybillNumber,
    string ContractNumber,
    DateTime? DispatchDate,
    WaybillStatus Status,
    
    string ConsumerName,
    string ConsumerAddress,
    string ConsumerBankAccount,
    
    TransportType TransportTypeCode,
    string TransportType,
    string TransportDetails,
    
    IEnumerable<WaybillItemDto> Items,

    decimal ProductsTotalSum,
    decimal TransportServiceSum,
    decimal TotalAmount,

    decimal DeliveryBaseCost,
    decimal DeliveryCommissionCost,
    decimal DeliveryTransportMultiplier
);