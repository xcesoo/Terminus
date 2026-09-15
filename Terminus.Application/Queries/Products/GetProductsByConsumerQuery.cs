using MediatR;
using Terminus.Application.DTOs;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Products;

public readonly record struct GetProductsByConsumerQuery(Guid ConsumerId) : IRequest<IReadOnlyCollection<ProductDto>>;

public class GetProductsByConsumerQueryHandler(IContractRepository contractRepository)
    : IRequestHandler<GetProductsByConsumerQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsByConsumerQuery request,
        CancellationToken cancellationToken)
    {
        var contracts =
            await contractRepository.GetContractsByConsumerIdWithProductsAsync(request.ConsumerId, cancellationToken);

        var result = contracts
            .Select(c => c.Product)
            .DistinctBy(p => p.Id) 
            .Select(p => new ProductDto(p.Id, p.Code, p.Name, p.Price, p.PriceListNumber))
            .ToList();

        return result.AsReadOnly();
    }
}