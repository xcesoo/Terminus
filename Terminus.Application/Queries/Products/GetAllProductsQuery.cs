using MediatR;
using Terminus.Application.DTOs;
using Terminus.Application.Extensions;
using Terminus.Domain.Interfaces.Repositories;

namespace Terminus.Application.Queries.Products;

public readonly record struct GetAllProductsQuery() : IRequest<IReadOnlyCollection<ProductDto>>;

public class GetAllProductsQueryHandler(IProductRepository productRepository) 
    : IRequestHandler<GetAllProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => p.MapToDto()).ToList().AsReadOnly();
    }
}