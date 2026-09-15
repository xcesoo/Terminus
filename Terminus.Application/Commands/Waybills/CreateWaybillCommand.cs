using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Waybills;

public readonly record struct CreateWaybillCommand(
    Guid ContractId,
    string WaybillNumber,
    int ShippedQuantity,
    string TransportType, 
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
        var contract = await contractRepository.GetByIdAsync(request.ContractId, cancellationToken);
        
        if (contract == null)
            throw new KeyNotFoundException("Договір не знайдено.");

        var context = new CreateWaybillContext(contract, request.ShippedQuantity);

        var validationResult = ruleEngine.Verify(context);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        Waybill waybill = request.TransportType.ToLower() switch
        {
            "auto" => AutoWaybill.Create(request.WaybillNumber, request.ContractId, request.ShippedQuantity,  
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum),
                
            "train" => TrainWaybill.Create(request.WaybillNumber, request.ContractId, request.ShippedQuantity,  
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum),
                
            "avia" => AviaWaybill.Create(request.WaybillNumber, request.ContractId, request.ShippedQuantity,  
                request.TransportIdentifier!, request.ReceiptNumber!, request.ServiceSum),
                
            _ => throw new ArgumentException("Невідомий тип транспорту")
        };

        await waybillRepository.AddAsync(waybill, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return waybill.Id;
    }
}