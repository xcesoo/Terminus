using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Consumers;

public readonly record struct ChangeConsumerBankAccountCommand(Guid Id, string BankAccount) : IRequest;

public class ChangeConsumerBankAccountCommandHandler(
    IConsumerRepository consumerRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeConsumerBankAccountCommand>
{
    public async Task Handle(ChangeConsumerBankAccountCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Споживача не знайдено.");

        consumer.ChangeBankAccount(request.BankAccount);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}