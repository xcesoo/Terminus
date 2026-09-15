namespace Terminus.Application.DTOs;

public record ContractDto(
    Guid Id, 
    string ContractNumber, 
    Guid ConsumerId, 
    Guid ProductId, 
    int Quantity, 
    DateTime? ConclusionDate);