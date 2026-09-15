namespace Terminus.Application.DTOs;

public record ProductDto(Guid Id, string Code, string Name, decimal Price, string PriceListNumber);