using Terminus.Domain.Enums;

namespace Terminus.Application.DTOs;

public record ContractDto(Guid Id, string ContractNumber, ContractStatus Status, Guid ConsumerId, DateTime? ConclusionDate, IEnumerable<ContractItemDto> Items);