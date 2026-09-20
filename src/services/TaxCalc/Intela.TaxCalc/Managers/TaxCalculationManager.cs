using Intela.TaxCalc.Models;
using Intela.TaxCalc.Providers;
using Intela.TaxCalc.Strategy;

namespace Intela.TaxCalc.Managers;

/// <summary>Use-case orchestration: strategy computes, provider saves. Handlers stay thin.</summary>
public sealed class TaxCalculationManager
{
    private readonly ITaxStrategy _strategy;
    private readonly ICalculationProvider _provider;

    public TaxCalculationManager(ITaxStrategy strategy, ICalculationProvider provider)
    {
        _strategy = strategy;
        _provider = provider;
    }

    public async Task<TaxCalculation> RunAsync(IReadOnlyList<TrialBalanceLine> lines, CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("At least one trial-balance line is required.");
        }

        var calculation = _strategy.Compute(lines);
        await _provider.SaveAsync(calculation, cancellationToken);
        return calculation;
    }

    public Task<TaxCalculation?> GetAsync(string id, CancellationToken cancellationToken) =>
        _provider.GetAsync(id, cancellationToken);
}
