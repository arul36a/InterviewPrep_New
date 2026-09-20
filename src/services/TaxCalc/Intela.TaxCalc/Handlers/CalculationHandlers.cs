using Intela.TaxCalc.Managers;
using Intela.TaxCalc.Models;
using MediatR;

namespace Intela.TaxCalc.Handlers;

public sealed record RunCalculationCommand(IReadOnlyList<TrialBalanceLine> Lines) : IRequest<TaxCalculation>;

public sealed record GetCalculationQuery(string Id) : IRequest<TaxCalculation?>;

public sealed class RunCalculationHandler : IRequestHandler<RunCalculationCommand, TaxCalculation>
{
    private readonly TaxCalculationManager _manager;

    public RunCalculationHandler(TaxCalculationManager manager) => _manager = manager;

    public Task<TaxCalculation> Handle(RunCalculationCommand request, CancellationToken cancellationToken) =>
        _manager.RunAsync(request.Lines, cancellationToken);
}

public sealed class GetCalculationHandler : IRequestHandler<GetCalculationQuery, TaxCalculation?>
{
    private readonly TaxCalculationManager _manager;

    public GetCalculationHandler(TaxCalculationManager manager) => _manager = manager;

    public Task<TaxCalculation?> Handle(GetCalculationQuery request, CancellationToken cancellationToken) =>
        _manager.GetAsync(request.Id, cancellationToken);
}
