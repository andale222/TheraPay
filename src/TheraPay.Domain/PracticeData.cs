namespace TheraPay.Domain;

public class PracticeData
{
    public string Name { get; set; } = "practice name placeholde";
    public string FirstNamePractitioner { get; set; } = "first name practitioner placeholder";
    public string LastNamePractitioner { get; set; } = "last name practitioner placeholder";
    public string Street { get; set; } = "street placeholder";
    public string HouseNumber { get; set; } = "1";
    public string PostalCode { get; set; } = "postal code placeholder";
    public string City { get; set; } = "city placeholder";
    public string? Country { get; set; }
    public string? AddressAdditional { get; set; }
    public string PhoneNumber { get; set; } = "phone number placeholder";
    public string IBAN { get; set; } = "IBAN placeholder";
    public string? BLZ { get; set; }
    public string? BankName { get; set; }
    public string? Subject { get; set; }
    public string TaxIdentificationNumber { get; set; } = "tax identification number placeholder";
    public int DefaultPaymentTermDays { get; set; } = 14;
    public InvoiceNumberState InvoiceNumberState { get; set; } = new InvoiceNumberState();
}
