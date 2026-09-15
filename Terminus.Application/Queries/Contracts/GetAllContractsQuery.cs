using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Contracts;

public readonly record struct GetAllContractsQuery() : IRequest<IReadOnlyCollection<ContractDto>>;

public class GetAllContractsQueryHandler(IContractRepository contractRepository) 
    : IRequestHandler<GetAllContractsQuery, IReadOnlyCollection<ContractDto>>
{
    public async Task<IReadOnlyCollection<ContractDto>> Handle(GetAllContractsQuery request, CancellationToken cancellationToken)
    {
        var contracts = await contractRepository.GetAllAsync(cancellationToken);
        return contracts.Select(c => c.MapToDto()).ToList().AsReadOnly();
    }
}