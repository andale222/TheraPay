namespace TheraPay.Domain;

public sealed record PaymentDetails
{
    public string IBAN { get; init; }
    public string? BLZ { get; init; }
    public string? BankName { get; init; }
    public string? Subject { get; init; }

    public PaymentDetails(string iban, string? blz = null, string? bankName = null, string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN is required.", nameof(iban));

        IBAN = iban;
        BLZ = blz;
        BankName = bankName;
        Subject = subject;
    }
}
