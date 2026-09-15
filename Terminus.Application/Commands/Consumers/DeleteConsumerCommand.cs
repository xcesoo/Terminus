using MediatR;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Commands.Consumers;

public readonly record struct DeleteConsumerCommand(Guid Id) : IRequest;

public class DeleteConsumerCommandHandler(IConsumerRepository consumerRepository) 
    : IRequestHandler<DeleteConsumerCommand>
{
    public async Task Handle(DeleteConsumerCommand request, CancellationToken cancellationToken)
    {
        var consumer = await consumerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Споживача не знайдено.");

        await consumerRepository.DeleteAsync(consumer, cancellationToken);
    }
}