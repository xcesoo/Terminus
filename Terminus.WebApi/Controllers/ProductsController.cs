using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Products;
using Terminus.Application.Queries.Products;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllProductsQuery(), token));
    }

    [HttpGet("consumer/{consumerId:guid}")]
    public async Task<IActionResult> GetByConsumer(Guid consumerId, CancellationToken token)
    {
        return Ok(await sender.Send(new GetProductsByConsumerQuery(consumerId), token));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/products/{id}", new { Id = id });
    }

    [HttpPatch("code")]
    public async Task<IActionResult> ChangeCode([FromBody] ChangeProductCodeCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("name")]
    public async Task<IActionResult> ChangeName([FromBody] ChangeProductNameCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("price")]
    public async Task<IActionResult> UpdatePrice([FromBody] UpdateProductPriceCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await sender.Send(new DeleteProductCommand(id), token);
        return NoContent();
    }
}