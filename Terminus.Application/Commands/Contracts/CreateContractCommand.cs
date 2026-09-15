using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Contracts;

public readonly record struct CreateContractCommand(
    string ContractNumber,
    Guid ConsumerId,
    Guid ProductId,
    int Quantity,
    DateTime ConclusionDate) : IRequest<Guid>;

public class CreateContractCommandHandler(
    IConsumerRepository consumerRepository,
    IProductRepository productRepository,
    IContractRepository contractRepository,
    IRuleEngine ruleEngine,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateContractCommand, Guid>
{
    public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.ConsumerId, cancellationToken)
            ?? throw new KeyNotFoundException("Споживача не знайдено.");
        
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Виріб не знайдено.");

        var ruleContext = new CreateContractContext(consumer, product, request.Quantity, request.ConclusionDate);
        
        var validationResult = ruleEngine.Verify(ruleContext);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        var contract = Contract.Create(request.ContractNumber, request.ConsumerId, request.ProductId, request.Quantity, request.ConclusionDate);
        
        await contractRepository.AddAsync(contract, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return contract.Id;
    }
}