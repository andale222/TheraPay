namespace TheraPay.Domain;

public sealed record PracticeDataRecord
{
    public string PracticeName { get; init; } = "";
    public string PracticeDescription { get; init; } = "";
    public string PracticePhoneNr { get; init; } = "";
    public string PracticeEmail { get; init; } = "";
    public string PractitionerFirstLastName { get; init; } = "";
    public Address Address { get; init; } = new Address("street", "1", "00000", "city");
    public string TaxNumber { get; init; } = "";
    public PaymentDetails PaymentDetails { get; init; } = new PaymentDetails("DE00");
    public int DefaultPaymentTermDays { get; init; }

    public static PracticeDataRecord FromPracticeData(PracticeData practiceData)
    {
        if (practiceData == null)
            throw new ArgumentNullException(nameof(practiceData));

        return new PracticeDataRecord
        {
            PracticeName = practiceData.Name,
            PracticeDescription = practiceData.PracticeDescription,
            PractitionerFirstLastName = practiceData.FirstNamePractitioner + " " + practiceData.LastNamePractitioner,
            PracticePhoneNr = practiceData.PhoneNumber,
            PracticeEmail = practiceData.PracticeEmail,
            Address = new Address(
                practiceData.Street,
                practiceData.HouseNumber,
                practiceData.PostalCode,
                practiceData.City,
                practiceData.Country,
                practiceData.AddressAdditional),
            TaxNumber = practiceData.TaxIdentificationNumber,
            PaymentDetails = new PaymentDetails(
                practiceData.IBAN,
                practiceData.BLZ,
                practiceData.BankName,
                practiceData.Subject),
            DefaultPaymentTermDays = practiceData.DefaultPaymentTermDays
        };
    }
}
