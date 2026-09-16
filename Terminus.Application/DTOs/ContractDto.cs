namespace Terminus.Application.DTOs;

public record ContractDto(Guid Id, string ContractNumber, Guid ConsumerId, DateTime? ConclusionDate, IEnumerable<ContractItemDto> Items);