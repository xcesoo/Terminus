using MediatR;
using Microsoft.AspNetCore.Mvc;
using Terminus.Application.Commands.Waybills;
using Terminus.Application.DTOs;
using Terminus.Application.Queries.Waybills;

namespace Terminus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaybillsController(ISender sender) : ControllerBase
{
    /// <summary>Отримати список усіх ТТН.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WaybillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await sender.Send(new GetAllWaybillsQuery(), token));
    }

    /// <summary>Отримати ТТН за ідентифікатором.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WaybillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await sender.Send(new GetWaybillByIdQuery(id), token));
    }

    /// <summary>Пошук ТТН за номером (у т.ч. частковий, нечіткий пошук).</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<WaybillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken token)
    {
        return Ok(await sender.Send(new SearchWaybillsQuery(term), token));
    }

    /// <summary>Задача 2: отримати список ТТН, відвантажених за добу.</summary>
    /// <param name="date">Будь-яка мить доби в UTC — враховується лише календарна дата. Приклад: 2026-09-17T00:00:00Z.</param>
    /// <param name="token"></param>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(IEnumerable<WaybillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime date, CancellationToken token)
    {
        return Ok(await sender.Send(new GetDailyWaybillsQuery(date), token));
    }

    /// <summary>Задача 3: отримати список ТТН, відвантажених за місяць.</summary>
    /// <param name="year">Рік, наприклад 2026.</param>
    /// <param name="month">Номер місяця від 1 до 12.</param>
    /// <param name="token"></param>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(IEnumerable<WaybillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthly([FromQuery] int year, [FromQuery] int month, CancellationToken token)
    {
        return Ok(await sender.Send(new GetMonthlyWaybillsQuery(year, month), token));
    }

    /// <summary>Задача 2: друкована відомість відвантаження виробів за добу (PDF).</summary>
    /// <param name="date">Будь-яка мить доби в UTC — враховується лише календарна дата. Приклад: 2026-09-17T00:00:00Z.</param>
    /// <param name="token"></param>
    [HttpGet("daily/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> GetDailyPdf([FromQuery] DateTime date, CancellationToken token)
    {
        var result = await sender.Send(new GetDailyShipmentStatementPdfQuery(date), token);
        return File(result.Content, "application/pdf", result.FileName);
    }

    /// <summary>Задача 3: друкована відомість відвантаження виробів за місяць (PDF).</summary>
    /// <param name="year">Рік, наприклад 2026.</param>
    /// <param name="month">Номер місяця від 1 до 12.</param>
    /// <param name="token"></param>
    [HttpGet("monthly/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyPdf([FromQuery] int year, [FromQuery] int month, CancellationToken token)
    {
        var result = await sender.Send(new GetMonthlyShipmentStatementPdfQuery(year, month), token);
        return File(result.Content, "application/pdf", result.FileName);
    }

    /// <summary>
    /// Оформити відвантаження за договором: створює ТТН (з реквізитами обраного виду транспорту)
    /// та автоматично розраховує вартість доставки.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateWaybillCommand command, CancellationToken token)
    {
        var id = await sender.Send(command, token);
        return Created($"/api/waybills/{id}", new { Id = id });
    }

    /// <summary>Відвантажити ТТН (переводить її зі статусу "Чернетка" у "Відвантажено").</summary>
    [HttpPatch("dispatch")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dispatch([FromBody] DispatchWaybillCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>Скасувати ТТН.</summary>
    [HttpPatch("cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel([FromBody] CancelWaybillCommand command, CancellationToken token)
    {
        await sender.Send(command, token);
        return NoContent();
    }

    /// <summary>
    /// Сформувати платіжну вимогу (PDF) за ТТН для направлення на юридичну адресу споживача.
    /// Доступно лише для ТТН у статусі "Відвантажено".
    /// </summary>
    [HttpGet("{id:guid}/payment-demand/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentDemandPdf(Guid id, CancellationToken token)
    {
        var result = await sender.Send(new GetPaymentDemandPdfQuery(id), token);

        return File(result.Content, "application/pdf", result.FileName);
    }
}
