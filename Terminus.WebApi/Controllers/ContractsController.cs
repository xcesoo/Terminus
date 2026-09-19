using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Contracts;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Contracts;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController(ISender sender) : ControllerBase
{
    /// <summary>Отримати список усіх договорів.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllContractsQuery(), token));
    }

    /// <summary>Отримати договір за ідентифікатором.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await sender.Send(new GetContractByIdQuery(id), token));
    }

    /// <summary>Пошук договорів за номером (у т.ч. частковий, нечіткий пошук).</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken token)
    {
        return Ok(await sender.Send(new SearchContractsQuery(term), token));
    }

    /// <summary>
    /// Сформувати друковану форму договору (PDF). Доступно лише для договорів у статусі "Підписано".
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken token)
    {
        var result = await sender.Send(new GetContractPdfQuery(id), token);
        return File(result.Content, "application/pdf", result.FileName);
    }

    /// <summary>Укласти новий договір зі споживачем на перелік виробів та їх кількість.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/contracts/{id}", new { Id = id });
    }

    /// <summary>Підписати договір (переводить його зі статусу "Чернетка" у "Підписано").</summary>
    [HttpPatch("sign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sign([FromBody] SignContractCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Розірвати договір.</summary>
    [HttpPatch("terminate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Terminate([FromBody] TerminateContractCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }
}
