using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Application.DTOs;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Contracts;

public readonly record struct CreateContractCommand(
    Guid ConsumerId,
    IEnumerable<ContractItemRequestDto> Items) : IRequest<Guid>; 

public class CreateContractCommandHandler(
    IConsumerRepository consumerRepository,
    IContractRepository contractRepository,
    IRuleEngine ruleEngine,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) 
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

        var datePart = timeProvider.GetUtcNow().ToString("yyyyMMdd");
        var randomLetters = new string(Enumerable.Range(0, 10).Select(_ => (char)Random.Shared.Next('A', 'Z' + 1)).ToArray());
        var generatedContractNumber = $"CTR-{datePart}-{randomLetters}";

        var contract = Contract.Create(generatedContractNumber, request.ConsumerId, contractItems);
        
        await contractRepository.AddAsync(contract, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return contract.Id;
    }
}