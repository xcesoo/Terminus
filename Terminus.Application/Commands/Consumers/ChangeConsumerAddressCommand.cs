using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Consumers;

public readonly record struct ChangeConsumerAddressCommand(Guid Id, string Address) : IRequest;

public class ChangeConsumerAddressCommandHandler(
    IConsumerRepository consumerRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ChangeConsumerAddressCommand>
{
    public async Task Handle(ChangeConsumerAddressCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Споживача не знайдено.");

        consumer.ChangeAddress(request.Address);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}