using Intela.TaxCalc.Models;

namespace Intela.TaxCalc.Strategy;

/// <summary>Pure tax rules. No HTTP, no storage, no MediatR.</summary>
public interface ITaxStrategy
{
    TaxCalculation Compute(IReadOnlyList<TrialBalanceLine> lines);
}

public sealed class FederalTaxStrategy : ITaxStrategy
{
    public const decimal Rate = 0.21m;

    public TaxCalculation Compute(IReadOnlyList<TrialBalanceLine> lines)
    {
        decimal Sum(string treatment) =>
            lines.Where(l => l.TaxTreatment.Equals(treatment, StringComparison.OrdinalIgnoreCase))
                .Sum(l => l.Amount);

        var book = Sum("Book");
        var permanent = Sum("Permanent");
        var temporary = Sum("Temporary");
        var taxable = book + permanent + temporary;

        return new TaxCalculation
        {
            BookIncome = book,
            PermanentDifferences = permanent,
            TemporaryDifferences = temporary,
            TaxableIncome = taxable,
            FederalTax = decimal.Round(taxable * Rate, 2, MidpointRounding.AwayFromZero)
        };
    }
}
