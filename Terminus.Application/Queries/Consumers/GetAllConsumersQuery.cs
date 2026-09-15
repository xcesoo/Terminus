using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Consumers;

public readonly record struct GetAllConsumersQuery() : IRequest<IReadOnlyCollection<ConsumerDto>>;

public class GetAllConsumersQueryHandler(IConsumerRepository consumerRepository) 
    : IRequestHandler<GetAllConsumersQuery, IReadOnlyCollection<ConsumerDto>>
{
    public async Task<IReadOnlyCollection<ConsumerDto>> Handle(GetAllConsumersQuery request, CancellationToken cancellationToken)
    {
        var consumers = await consumerRepository.GetAllAsync(cancellationToken);
        return consumers.Select(c => c.MapToDto()).ToList().AsReadOnly();
    }
}