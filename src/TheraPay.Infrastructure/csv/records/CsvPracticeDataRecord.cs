
public sealed class CsvPracticeDataRecord
{
    public string Name { get; set; } = string.Empty;
    public string FirstNamePractitioner { get; set; } = string.Empty;
    public string LastNamePractitioner { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? AddressAdditional { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string? BLZ { get; set; }
    public string? BankName { get; set; }
    public string? Subject { get; set; }
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public int DefaultPaymentTermDays { get; set; }
    public int InvoiceStateYear { get; set; }
    public int InvoiceStateRandomStart { get; set; }
    public int InvoiceStateNextIssueNumber { get; set; }
}
