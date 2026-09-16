using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Enums;
using Terminus.Domain.Interfaces;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Waybills;

public readonly record struct CreateWaybillCommand(
    Guid ContractId,
    IEnumerable<WaybillItemRequestDto> Items, 
    TransportType TransportType,
    string? TransportIdentifier, 
    string? ReceiptNumber
) : IRequest<Guid>;

public class CreateWaybillCommandHandler(
    IContractRepository contractRepository,
    IWaybillRepository waybillRepository,
    IUnitOfWork unitOfWork,
    IRuleEngine ruleEngine,
    IDeliveryCalculatorService deliveryCalculator,
    IDocumentNumberGenerator documentNumberGenerator)
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

        var totalQuantity = waybillItems.Sum(i => i.ShippedQuantity);
        decimal declaredValue = waybillItems.Sum(item => 
            contract.Items.First(ci => ci.ProductId == item.ProductId).Product.Price * item.ShippedQuantity);

        decimal serviceSum = deliveryCalculator.Calculate(request.TransportType, totalQuantity, declaredValue);

        var generatedWaybillNumber = documentNumberGenerator.GenerateWaybillNumber();

        Waybill waybill = request.TransportType switch
        {
            TransportType.Auto => AutoWaybill.Create(generatedWaybillNumber, request.ContractId, request.TransportIdentifier!, request.ReceiptNumber!, serviceSum, waybillItems),
            TransportType.Train => TrainWaybill.Create(generatedWaybillNumber, request.ContractId, request.TransportIdentifier!, request.ReceiptNumber!, serviceSum, waybillItems),
            TransportType.Avia => AviaWaybill.Create(generatedWaybillNumber, request.ContractId, request.TransportIdentifier!, request.ReceiptNumber!, serviceSum, waybillItems),
            _ => throw new ArgumentOutOfRangeException(nameof(request.TransportType), "Невідомий тип транспорту")
        };

        await waybillRepository.AddAsync(waybill, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return waybill.Id;
    }
}