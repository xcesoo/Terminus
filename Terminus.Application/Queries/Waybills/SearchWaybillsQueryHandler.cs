using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct SearchWaybillsQuery(string SearchTerm) : IRequest<IReadOnlyCollection<WaybillDto>>;

public class SearchWaybillsQueryHandler(IWaybillRepository waybillRepository) 
    : IRequestHandler<SearchWaybillsQuery, IReadOnlyCollection<WaybillDto>>
{
    public async Task<IReadOnlyCollection<WaybillDto>> Handle(SearchWaybillsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm)) return [];

        var waybills = await waybillRepository.SearchByNumberAsync(request.SearchTerm, cancellationToken);
        return waybills.Select(w => w.MapToDto()).ToList().AsReadOnly();
    }
}