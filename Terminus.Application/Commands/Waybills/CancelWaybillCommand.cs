using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Waybills;

public readonly record struct CancelWaybillCommand(Guid WaybillId) : IRequest;

public class CancelWaybillCommandHandler(
    IWaybillRepository waybillRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CancelWaybillCommand>
{
    public async Task Handle(CancelWaybillCommand request, CancellationToken cancellationToken)
    {
        var waybill = await waybillRepository.GetByIdAsync(request.WaybillId, cancellationToken)
            ?? throw new KeyNotFoundException("ТТН не знайдено.");

        waybill.Cancel();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}