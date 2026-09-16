using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetMonthlyWaybillsQuery(int Year, int Month) : IRequest<IReadOnlyCollection<WaybillDto>>;

public class GetMonthlyWaybillsQueryHandler(IWaybillRepository waybillRepository) 
    : IRequestHandler<GetMonthlyWaybillsQuery, IReadOnlyCollection<WaybillDto>>
{
    public async Task<IReadOnlyCollection<WaybillDto>> Handle(GetMonthlyWaybillsQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var endDate = new DateTime(request.Year, request.Month, daysInMonth, 0, 0, 0, DateTimeKind.Utc);

        var waybills = await waybillRepository.GetWaybillsByPeriodAsync(startDate, endDate, cancellationToken);
        return waybills.Select(w => w.MapToDto()).ToList().AsReadOnly();
    }
}