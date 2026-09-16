using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Contracts;

public readonly record struct CreateContractCommand(
    string ContractNumber,
    Guid ConsumerId,
    IEnumerable<ContractItemDto> Items) : IRequest<Guid>;

public class CreateContractCommandHandler(
    IConsumerRepository consumerRepository,
    IContractRepository contractRepository,
    IRuleEngine ruleEngine,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateContractCommand, Guid>
{
    public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.ConsumerId, cancellationToken)
                       ?? throw new KeyNotFoundException("Споживача не знайдено.");
        
        var contractItems = request.Items.Select(i => new ContractItem(i.ProductId, i.Quantity)).ToList();

        var ruleContext = new CreateContractContext(consumer, contractItems);
        var validationResult = ruleEngine.Verify(ruleContext);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        var contract = Contract.Create(request.ContractNumber, request.ConsumerId, contractItems);
        
        await contractRepository.AddAsync(contract, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return contract.Id;
    }
}