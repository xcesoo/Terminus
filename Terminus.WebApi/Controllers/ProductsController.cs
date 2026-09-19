using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Products;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Products;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>Отримати список усіх виробів.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllProductsQuery(), token));
    }

    /// <summary>
    /// Задача 4: сформувати список виробів, укладених у договорах конкретного споживача.
    /// </summary>
    [HttpGet("consumer/{consumerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByConsumer(Guid consumerId, CancellationToken token)
    {
        return Ok(await sender.Send(new GetProductsByConsumerQuery(consumerId), token));
    }

    /// <summary>Створити новий виріб. Код виробу має бути унікальним.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/products/{id}", new { Id = id });
    }

    /// <summary>Змінити код виробу. Новий код має бути унікальним.</summary>
    [HttpPatch("code")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeCode([FromBody] ChangeProductCodeCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Змінити найменування виробу.</summary>
    [HttpPatch("name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeName([FromBody] ChangeProductNameCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Змінити ціну виробу. Формує новий номер прейскуранта на дату зміни.</summary>
    [HttpPatch("price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice([FromBody] UpdateProductPriceCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Видалити виріб. Неможливо, якщо на нього посилаються існуючі договори/ТТН.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await sender.Send(new DeleteProductCommand(id), token);
        return NoContent();
    }
}
