using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Waybills;

public readonly record struct DispatchWaybillCommand(
    Guid WaybillId, 
    DateTime DispatchDate) : IRequest;

public class DispatchWaybillCommandHandler(
    IWaybillRepository waybillRepository,
    IRuleEngine ruleEngine,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<DispatchWaybillCommand>
{
    public async Task Handle(DispatchWaybillCommand request, CancellationToken cancellationToken)
    {
        var waybill = await waybillRepository.GetByIdAsync(request.WaybillId, cancellationToken)
                      ?? throw new KeyNotFoundException("ТТН не знайдено.");

        var utcDate = request.DispatchDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.DispatchDate, DateTimeKind.Utc) 
            : request.DispatchDate.ToUniversalTime();

        var context = new DispatchWaybillContext(waybill.Contract, utcDate);

        var validationResult = ruleEngine.Verify(context);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        waybill.Dispatch(utcDate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}