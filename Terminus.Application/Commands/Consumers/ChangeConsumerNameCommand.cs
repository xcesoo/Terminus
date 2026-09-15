using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Consumers;

public readonly record struct ChangeConsumerNameCommand(Guid Id, string Name) : IRequest;

public class ChangeConsumerNameCommandHandler(
    IConsumerRepository consumerRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeConsumerNameCommand>
{
    public async Task Handle(ChangeConsumerNameCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Споживача не знайдено.");

        consumer.ChangeName(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}