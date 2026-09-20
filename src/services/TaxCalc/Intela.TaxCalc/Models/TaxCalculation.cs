namespace Intela.TaxCalc.Models;

public sealed class TrialBalanceLine
{
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public decimal Amount { get; set; }
    /// <summary>Book | Permanent | Temporary</summary>
    public string TaxTreatment { get; set; } = "Book";
}

public sealed class TaxCalculation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public decimal BookIncome { get; set; }
    public decimal PermanentDifferences { get; set; }
    public decimal TemporaryDifferences { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal FederalTax { get; set; }
}

public sealed class RunCalculationRequest
{
    public List<TrialBalanceLine> Lines { get; set; } = [];
}
