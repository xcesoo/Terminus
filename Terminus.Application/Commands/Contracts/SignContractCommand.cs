using MediatR;
using Terminus.Application.Common.Rules;
using Terminus.Application.Common.Rules.Interfaces;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Contracts;

public readonly record struct SignContractCommand(
    Guid ContractId, 
    DateTime ConclusionDate) : IRequest;

public class SignContractCommandHandler(
    IContractRepository contractRepository,
    IRuleEngine ruleEngine,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<SignContractCommand>
{
    public async Task Handle(SignContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await contractRepository.GetByIdAsync(request.ContractId, cancellationToken)
                       ?? throw new KeyNotFoundException("Договір не знайдено.");

        var context = new SignContractContext(contract, request.ConclusionDate);

        var validationResult = ruleEngine.Verify(context);
        if (!validationResult.IsSuccess)
            throw new InvalidOperationException(validationResult.ErrorMessage);

        contract.Sign(request.ConclusionDate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}