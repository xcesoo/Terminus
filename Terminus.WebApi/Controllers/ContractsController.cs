using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Contracts;
using Terminus.Application.Queries.Contracts;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllContractsQuery(), token));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/contracts/{id}", new { Id = id });
    }

    [HttpPatch("sign")]
    public async Task<IActionResult> Sign([FromBody] SignContractCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("terminate")]
    public async Task<IActionResult> Terminate([FromBody] TerminateContractCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }
}