using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Contracts;

public readonly record struct TerminateContractCommand(Guid ContractId) : IRequest;

public class TerminateContractCommandHandler(
    IContractRepository contractRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<TerminateContractCommand>
{
    public async Task Handle(TerminateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await contractRepository.GetByIdAsync(request.ContractId, cancellationToken)
            ?? throw new KeyNotFoundException("Договір не знайдено.");

        contract.Terminate();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}