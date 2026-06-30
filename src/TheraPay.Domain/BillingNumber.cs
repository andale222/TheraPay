using System.Text.Json.Serialization;

namespace TheraPay.Domain;

public sealed record BillingNumber
{
    [JsonConstructor]
    public BillingNumber(
        string NumberIdentifier,
        decimal Factor,
        decimal BaseValue,
        string Description,
        BillingNumberType Type)
    {
        if (string.IsNullOrWhiteSpace(NumberIdentifier))
        {
            throw new ArgumentException("Number identifier cannot be empty.", nameof(NumberIdentifier));
        }

        if (Factor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Factor), "Factor cannot be negative.");
        }

        if (BaseValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BaseValue), "Base value cannot be negative.");
        }

        this.NumberIdentifier = NumberIdentifier.Trim();
        this.Factor = Factor;
        this.BaseValue = BaseValue;
        this.Description = Description?.Trim() ?? "";
        this.Type = Type;
    }

    public string NumberIdentifier { get; init; }
    public decimal Factor { get; init; }
    public decimal BaseValue { get; init; }
    public string Description { get; init; }
    public BillingNumberType Type { get; init; }

    [JsonIgnore]
    public decimal Amount => Math.Round(BaseValue * Factor, 2, MidpointRounding.AwayFromZero);
}

public enum BillingNumberType
{
    Privat,
    Selbstzahler,
    Kostenerstattung
}

public static class BillingNumberCatalog
{
    private static readonly IReadOnlyList<BillingNumber> SeedBillingNumbers =
    [
        // Temporary seed data until this catalogue is backed by an editable CSV database.
        new("0", 1.00m, 110.00m, "Psychotherapeutisches Erstgespräch", BillingNumberType.Privat),
        new("801a", 2.30m, 14.57m, "Erhebung des aktuellen psychischen Befundes", BillingNumberType.Privat),
        new("812a", 2.30m, 29.14m, "Psychotherapeutische Sprechstunde (25 Min.)", BillingNumberType.Privat),
        new("812a", 2.30m, 29.14m, "Psychotherapeutische Kurzzeittherapie (25 Min.)", BillingNumberType.Privat),
        new("860", 2.30m, 53.62m, "Biographische Anamnese", BillingNumberType.Privat),
        new("861", 2.30m, 40.22m, "Tiefenpsychologisch fundierte Psychotherapie, Einzelbehandlung (50 Min.)", BillingNumberType.Privat),
        new("870", 2.30m, 43.72m, "Verhaltenstherapie, Einzelbehandlung (50 Min.)", BillingNumberType.Privat),
        new("870", 1.90m, 43.72m, "Psychotherapeutische Behandlung", BillingNumberType.Kostenerstattung),
        new("S-01", 1.00m, 100.00m, "Selbstzahler-Sitzung", BillingNumberType.Selbstzahler)
    ];

    public static IReadOnlyList<BillingNumber> GetDefaultNumbers()
    {
        return SeedBillingNumbers;
    }

    public static BillingNumber? FindByIdentifier(string numberIdentifier, BillingNumberType? type = null)
    {
        return SeedBillingNumbers.FirstOrDefault(billingNumber =>
            string.Equals(billingNumber.NumberIdentifier, numberIdentifier, StringComparison.OrdinalIgnoreCase)
            && (type is null || billingNumber.Type == type));
    }
}
