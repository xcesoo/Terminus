using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Consumers;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Consumers;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumersController(ISender sender) : ControllerBase
{
    /// <summary>Отримати список усіх споживачів.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConsumerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllConsumersQuery(), token));
    }

    /// <summary>
    /// Задача 1: сформувати список виробів разом зі споживачами, яким вони відвантажувались
    /// (за укладеними договорами).
    /// </summary>
    [HttpGet("products-with-consumers")]
    [ProducesResponseType(typeof(IEnumerable<ProductWithConsumersDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsWithConsumers(CancellationToken token)
    {
        return Ok(await sender.Send(new GetProductsWithConsumersQuery(), token));
    }

    /// <summary>Зареєструвати нового споживача.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateConsumerCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/consumers/{id}", new { Id = id });
    }

    /// <summary>Змінити назву споживача.</summary>
    [HttpPatch("name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeName([FromBody] ChangeConsumerNameCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Змінити юридичну адресу споживача.</summary>
    [HttpPatch("address")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeAddress([FromBody] ChangeConsumerAddressCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Змінити розрахунковий рахунок споживача.</summary>
    [HttpPatch("bank-account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeBankAccount([FromBody] ChangeConsumerBankAccountCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Видалити споживача. Неможливо, якщо на нього посилаються існуючі договори.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await sender.Send(new DeleteConsumerCommand(id), token);
        return NoContent();
    }
}
