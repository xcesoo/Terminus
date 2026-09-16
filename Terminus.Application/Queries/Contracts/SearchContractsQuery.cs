using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Contracts;

public readonly record struct SearchContractsQuery(string SearchTerm) : IRequest<IReadOnlyCollection<ContractDto>>;

public class SearchContractsQueryHandler(IContractRepository contractRepository) 
    : IRequestHandler<SearchContractsQuery, IReadOnlyCollection<ContractDto>>
{
    public async Task<IReadOnlyCollection<ContractDto>> Handle(SearchContractsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm)) return [];

        var contracts = await contractRepository.SearchByNumberAsync(request.SearchTerm, cancellationToken);
        return contracts.Select(c => c.MapToDto()).ToList().AsReadOnly();
    }
}