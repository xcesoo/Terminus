namespace Terminus.Application.DTOs;

public record WaybillItemDto(
    Guid ProductId, 
    string ProductName,
    decimal Price,           
    int ShippedQuantity,     
    decimal TotalPrice       
);