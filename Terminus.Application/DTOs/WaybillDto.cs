namespace Terminus.Application.DTOs;

public record WaybillDto(string WaybillNumber, DateTime? DispatchDate, string ConsumerName, string TransportType, string TransportDetails, decimal ServiceSum, IEnumerable<WaybillItemDto> Items);