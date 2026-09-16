namespace Terminus.Application.DTOs;

public record WaybillItemRequestDto(Guid ProductId, int ShippedQuantity);