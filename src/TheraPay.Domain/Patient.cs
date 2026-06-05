using System.Text.RegularExpressions;

namespace TheraPay.Domain;

public class Patient
{
    public const string PhoneNumberPattern = @"^\+?\d+$";
    public static readonly Regex PhoneNumberRegex = new(PhoneNumberPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public static readonly Regex EmailRegex = new(EmailPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public const string Icd10DiagnosisPattern = @"^[A-TV-Z][0-9][A-Z0-9](?:\.[A-Z0-9]{1,4})?$";
    public static readonly Regex Icd10DiagnosisRegex = new(Icd10DiagnosisPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public string LastName { get; private set; } = "";
    public string FirstName { get; private set; } = "";
    public string ID { get; init; } = "";
    public bool IsActive { get; set; } = true;
    public Address? Address { get; private set; }
    public PatientInsuranceStatus InsuranceStatus { get; private set; } = PatientInsuranceStatus.Privat;
    public string ICD10Diagnosis { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string PhoneNumber { get; private set; } = "";



    public Patient(string firstName, string lastName, string id)
    {
        FirstName = firstName;
        LastName = lastName;
        ID = id;
    }

    public void SetAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void SetAddress(
        string street,
        string houseNumber,
        string postalCode,
        string place,
        string? country = null,
        string? additionalInfo = null)
    {
        Address = new Address(street, houseNumber, postalCode, place, country, additionalInfo);
    }

    public void SetInsuranceStatus(PatientInsuranceStatus insuranceStatus)
    {
        InsuranceStatus = insuranceStatus;
    }

    public void SetPhoneNumber(string newPhoneNumber)
    {
        string value = newPhoneNumber.Trim();
        if (string.IsNullOrWhiteSpace(value) || !PhoneNumberRegex.IsMatch(value))
            throw new ArgumentException("Phone number may only contain digits and an optional leading '+'.", nameof(newPhoneNumber));

        PhoneNumber = value;
    }

    public void SetICD10Diagnosis(string newDiagnosis)
    {
        string value = newDiagnosis.Trim();
        if (string.IsNullOrWhiteSpace(value) || !Icd10DiagnosisRegex.IsMatch(value))
            throw new ArgumentException("Diagnosis must be a valid ICD-10-CM code.", nameof(newDiagnosis));

        ICD10Diagnosis = value;
    }

    public void SetEmail(string newEmail)
    {
        string value = newEmail.Trim();
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
            throw new ArgumentException("Email must be in a valid format like xxx@xx.xx.", nameof(newEmail));

        Email = value;
    }
}
