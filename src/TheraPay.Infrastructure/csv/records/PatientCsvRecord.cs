namespace TheraPay.Infrastructure.csv;

public sealed class PatientCsvRecord
{
    public string Id { get; set; } = "";
    public string Salutation { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public string InsuranceStatus { get; set; } = "";
    public string Street { get; set; } = "";
    public string HouseNumber { get; set; } = "";
    public string StreetAndNumber { get; set; } = "";
    public string ICD10Diagnosis { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Place { get; set; } = "";
    public string Country { get; set; } = "";
    public string Email { get; set; } = "";
    public string AdditionalInfo { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
