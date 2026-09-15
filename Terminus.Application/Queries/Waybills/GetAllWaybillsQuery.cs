using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetAllWaybillsQuery() : IRequest<IReadOnlyCollection<WaybillDto>>;

public class GetAllWaybillsQueryHandler(IWaybillRepository waybillRepository) 
    : IRequestHandler<GetAllWaybillsQuery, IReadOnlyCollection<WaybillDto>>
{
    public async Task<IReadOnlyCollection<WaybillDto>> Handle(GetAllWaybillsQuery request, CancellationToken cancellationToken)
    {
        var waybills = await waybillRepository.GetAllAsync(cancellationToken);
        return waybills.Select(w => w.MapToDto()).ToList().AsReadOnly();
    }
}