using Terminus.Domain.Enums;

namespace Terminus.Application.DTOs;

public record ContractDto(
    Guid Id, 
    string ContractNumber, 
    Guid ConsumerId, 
    string Status, 
    DateTime? ConclusionDate, 
    IEnumerable<ContractItemDto> Items,
    decimal TotalAmount);