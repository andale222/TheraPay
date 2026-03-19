namespace TheraPay.Domain;

public class PracticeData
{
    public string Name { get; set; } = "practice name placeholde";
    public string FirstNamePractitioner { get; set; } = "first name practitioner placeholder";
    public string LastNamePractitioner { get; set; } = "last name practitioner placeholder";
    public string StreetAndNumber { get; set; } = "street and number placeholder";
    public string CityAndPostalCode { get; set; } = "city and postal code placeholder";
    public string Country { get; set; } = "country placeholder";
    public string PhoneNumber { get; set; } = "phone number placeholder";
    public string IBAN { get; set; } = "IBAN placeholder";
    public string TaxIdentificationNumber { get; set; } = "tax identification number placeholder";
    public int DefaultPaymentTermDays { get; set; } = 14;
}
