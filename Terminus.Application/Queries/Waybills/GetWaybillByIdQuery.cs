using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Waybills;

public readonly record struct GetWaybillByIdQuery(Guid Id) : IRequest<WaybillDto>;

public class GetWaybillByIdQueryHandler(IWaybillRepository waybillRepository) 
    : IRequestHandler<GetWaybillByIdQuery, WaybillDto>
{
    public async Task<WaybillDto> Handle(GetWaybillByIdQuery request, CancellationToken cancellationToken)
    {
        var waybill = await waybillRepository.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw new KeyNotFoundException("ТТН не знайдено.");

        return waybill.MapToDto();
    }
}