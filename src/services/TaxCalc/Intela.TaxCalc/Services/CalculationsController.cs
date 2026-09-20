using Intela.TaxCalc.Handlers;
using Intela.TaxCalc.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Intela.TaxCalc.Services;

/// <summary>HTTP only. No business rules here.</summary>
[ApiController]
[Route("api/calculations")]
public sealed class CalculationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CalculationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<TaxCalculation>> Run([FromBody] RunCalculationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RunCalculationCommand(request.Lines), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaxCalculation>> Get(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCalculationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
