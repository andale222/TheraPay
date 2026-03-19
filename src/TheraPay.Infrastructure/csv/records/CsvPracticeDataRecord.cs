
public sealed class CsvPracticeDataRecord
{
    public string Name { get; set; } = string.Empty;
    public string FirstNamePractitioner { get; set; } = string.Empty;
    public string LastNamePractitioner { get; set; } = string.Empty;
    public string StreetAndNumber { get; set; } = string.Empty;
    public string CityAndPostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public int DefaultPaymentTermDays { get; set; }
}
