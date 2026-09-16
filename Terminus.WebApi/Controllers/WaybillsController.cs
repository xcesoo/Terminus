using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Waybills;
using Terminus.Application.Queries.Waybills;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaybillsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllWaybillsQuery(), token));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await sender.Send(new GetWaybillByIdQuery(id), token));
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken token)
    {
        return Ok(await sender.Send(new SearchWaybillsQuery(term), token));
    }
    
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime date, CancellationToken token) 
    {
        return Ok(await sender.Send(new GetDailyWaybillsQuery(date), token)); 
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month, CancellationToken token) 
    {
        return Ok(await sender.Send(new GetMonthlyWaybillsQuery(year, month), token)); 
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWaybillCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/waybills/{id}", new { Id = id });
    }

    [HttpPatch("dispatch")]
    public async Task<IActionResult> Dispatch([FromBody] DispatchWaybillCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    [HttpPatch("cancel")]
    public async Task<IActionResult> Cancel([FromBody] CancelWaybillCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }
    
    [HttpGet("{id:guid}/payment-demand/pdf")]
    public async Task<IActionResult> GetPaymentDemandPdf(Guid id, CancellationToken token)
    {
        return Ok("pdf"); //todo
    }
}