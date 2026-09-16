using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Consumers;
using Terminus.Application.Queries.Consumers;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllConsumersQuery(), token));
    }

    [HttpGet("products-with-consumers")]
    public async Task<IActionResult> GetProductsWithConsumers(CancellationToken token)
    {
        return Ok(await sender.Send(new GetProductsWithConsumersQuery(), token));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConsumerCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/consumers/{id}", new { Id = id });
    }

    [HttpPatch("name")]
    public async Task<IActionResult> ChangeName([FromBody] ChangeConsumerNameCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("address")]
    public async Task<IActionResult> ChangeAddress([FromBody] ChangeConsumerAddressCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("bank-account")]
    public async Task<IActionResult> ChangeBankAccount([FromBody] ChangeConsumerBankAccountCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await sender.Send(new DeleteConsumerCommand(id), token);
        return NoContent();
    }
}