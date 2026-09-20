using Intela.TaxCalc.Models;
using Intela.TaxCalc.Strategy;

namespace Intela.TaxCalc.Tests;

public sealed class FederalTaxStrategyTests
{
    [Fact]
    public void Computes_taxable_income_and_21_percent_federal_tax()
    {
        var strategy = new FederalTaxStrategy();

        var result = strategy.Compute(
        [
            new TrialBalanceLine { AccountCode = "4000", Amount = 100_000m, TaxTreatment = "Book" },
            new TrialBalanceLine { AccountCode = "5000", Amount = 5_000m, TaxTreatment = "Permanent" },
            new TrialBalanceLine { AccountCode = "6000", Amount = 2_000m, TaxTreatment = "Temporary" }
        ]);

        Assert.Equal(100_000m, result.BookIncome);
        Assert.Equal(107_000m, result.TaxableIncome);
        Assert.Equal(22_470m, result.FederalTax);
    }
}
