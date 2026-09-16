using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Waybills;

public readonly record struct CreateWaybillCommand(
    Guid ContractId,
    string WaybillNumber,
    IEnumerable<WaybillItemDto> Items, 
    TransportType TransportType,
    string? TransportIdentifier, 
    string? ReceiptNumber,
    decimal ServiceSum
) : IRequest<Guid>;

public class CreateWaybillCommandHandler(
    IContractRepository contractRepository,
    IWaybillRepository waybillRepository,
    IUnitOfWork unitOfWork,
    IRuleEngine ruleEngine)
    : IRequestHandler<CreateWaybillCommand, Guid>
{
    public async Task<Guid> Handle(CreateWaybillCommand request, CancellationToken cancellationToken)
    {
        var contract = await contractRepository.GetByIdAsync(request.ContractId, cancellationToken)
            ?? throw new KeyNotFoundException("Договір не знайдено.");

        var waybillItems = request.Items
            .Select(i => new WaybillItem(i.ProductId, i.ShippedQuantity))
            .ToList();

        var context = new CreateWaybillContext(contract, waybillItems);

        var validationResult = ruleEngine.Verify(context);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        Waybill waybill = request.TransportType switch
        {
            TransportType.Auto => AutoWaybill.Create(
                request.WaybillNumber, request.ContractId, 
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum, waybillItems),
                
            TransportType.Train => TrainWaybill.Create(
                request.WaybillNumber, request.ContractId, 
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum, waybillItems),
                
            TransportType.Avia => AviaWaybill.Create(
                request.WaybillNumber, request.ContractId, 
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum, waybillItems),
                
            _ => throw new ArgumentOutOfRangeException(nameof(request.TransportType), "Невідомий тип транспорту")
        };

        await waybillRepository.AddAsync(waybill, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return waybill.Id;
    }
}