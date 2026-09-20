using Intela.TaxCalc.Models;

namespace Intela.TaxCalc.Providers;

/// <summary>Persistence. Swap InMemory for Mongo in a later phase without touching handlers.</summary>
public interface ICalculationProvider
{
    Task SaveAsync(TaxCalculation calculation, CancellationToken cancellationToken);
    Task<TaxCalculation?> GetAsync(string id, CancellationToken cancellationToken);
}

public sealed class InMemoryCalculationProvider : ICalculationProvider
{
    private readonly Dictionary<string, TaxCalculation> _store = new();
    private readonly object _lock = new();

    public Task SaveAsync(TaxCalculation calculation, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store[calculation.Id] = calculation;
        }

        return Task.CompletedTask;
    }

    public Task<TaxCalculation?> GetAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _store.TryGetValue(id, out var value);
            return Task.FromResult(value);
        }
    }
}
