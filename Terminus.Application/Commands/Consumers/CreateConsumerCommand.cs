using MediatR;
using Terminus.Domain.Entities;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Consumers;

public readonly record struct CreateConsumerCommand(
    string Name, 
    string Address, 
    string BankAccount) : IRequest<Guid>;

public class CreateConsumerCommandHandler(
    IConsumerRepository consumerRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateConsumerCommand, Guid>
{
    public async Task<Guid> Handle(CreateConsumerCommand request, CancellationToken cancellationToken)
    {
        var consumer = Consumer.Create(request.Name, request.Address, request.BankAccount);
        
        await consumerRepository.AddAsync(consumer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return consumer.Id;
    }
}