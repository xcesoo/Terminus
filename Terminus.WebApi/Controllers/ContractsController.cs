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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await sender.Send(new GetContractByIdQuery(id), token));
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken token)
    {
        return Ok(await sender.Send(new SearchContractsQuery(term), token));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken token)
    {
        var result = await sender.Send(new GetContractPdfQuery(id), token);
        return File(result.Content, "application/pdf", result.FileName);
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