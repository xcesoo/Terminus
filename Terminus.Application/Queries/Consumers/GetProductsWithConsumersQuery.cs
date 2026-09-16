using MediatR;
using Terminus.Application.DTOs;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Consumers;

public readonly record struct GetProductsWithConsumersQuery() : IRequest<IReadOnlyCollection<ProductWithConsumersDto>>;

public class GetProductsWithConsumersQueryHandler(IContractRepository contractRepository) 
    : IRequestHandler<GetProductsWithConsumersQuery, IReadOnlyCollection<ProductWithConsumersDto>>
{
    public async Task<IReadOnlyCollection<ProductWithConsumersDto>> Handle(GetProductsWithConsumersQuery request, CancellationToken cancellationToken)
    {
        var contracts = await contractRepository.GetContractsWithProductsAndConsumersAsync(cancellationToken);

        var result = contracts
            .SelectMany(c => c.Items.Select(i => new { c.Consumer, i.Product }))
            .GroupBy(x => x.Product)
            .Select(g => new ProductWithConsumersDto(
                ProductName: g.Key.Name,
                ConsumerNames: g.Select(x => x.Consumer.Name).Distinct()
            ))
            .ToList();

        return result.AsReadOnly();
    }
}