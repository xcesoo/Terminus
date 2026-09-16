namespace Terminus.Application.DTOs;

public record ContractItemDto(
    Guid ProductId, 
    string ProductName, 
    decimal Price, 
    int Quantity, 
    decimal TotalPrice);