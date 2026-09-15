using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetDailyWaybillsQuery(DateTime Date) : IRequest<IReadOnlyCollection<WaybillDto>>;

public class GetDailyWaybillsQueryHandler(IWaybillRepository waybillRepository) 
    : IRequestHandler<GetDailyWaybillsQuery, IReadOnlyCollection<WaybillDto>>
{
    public async Task<IReadOnlyCollection<WaybillDto>> Handle(GetDailyWaybillsQuery request, CancellationToken cancellationToken)
    {
        var waybills = await waybillRepository.GetWaybillsByDateAsync(request.Date, cancellationToken);
        return waybills.Select(w => w.MapToDto()).ToList().AsReadOnly();
    }
}