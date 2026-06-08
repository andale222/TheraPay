using TheraPay.Domain;
using CsvHelper.Configuration;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvPatientStore(string filePath) : CsvStore<Patient, PatientCsvRecord>(filePath)
{
    protected override CsvConfiguration CreateCsvConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };
    }

    protected override PatientCsvRecord ToRecord(Patient patient)
    {
        return new PatientCsvRecord
        {
            Id = patient.ID,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            InsuranceStatus = patient.InsuranceStatus.ToString(),
            Street = patient.Address?.Street ?? "",
            HouseNumber = patient.Address?.HouseNumber ?? "",
            StreetAndNumber = patient.Address?.GetStreetNr() ?? "",
            ICD10Diagnosis = patient.ICD10Diagnosis,
            PostalCode = patient.Address?.PostalCode ?? "",
            Place = patient.Address?.City ?? "",
            Country = patient.Address?.Country ?? "",
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            AdditionalInfo = patient.Address?.Additional ?? "",
        };
    }

    protected override Patient ToDomain(PatientCsvRecord record)
    {
        var patient = new Patient(record.FirstName, record.LastName, record.Id);
        var (street, houseNumber) = GetStreetAndHouseNumber(record);

        if (HasCompleteAddress(street, houseNumber, record.PostalCode, record.Place))
            patient.SetAddress(street, houseNumber, record.PostalCode, record.Place, record.Country, record.AdditionalInfo);
        if (string.IsNullOrWhiteSpace(record.ICD10Diagnosis) == false)
            patient.SetICD10Diagnosis(record.ICD10Diagnosis);
        if (string.IsNullOrWhiteSpace(record.Email) == false)
            patient.SetEmail(record.Email);
        if (string.IsNullOrWhiteSpace(record.PhoneNumber) == false)
            patient.SetPhoneNumber(record.PhoneNumber);
        if (TryParseInsuranceStatus(record.InsuranceStatus, out var insuranceStatus))
            patient.SetInsuranceStatus(insuranceStatus);

        return patient;
    }

    private static bool TryParseInsuranceStatus(string insuranceStatus, out PatientInsuranceStatus parsedStatus)
    {
        return Enum.TryParse(insuranceStatus, ignoreCase: true, out parsedStatus);
    }

    private static bool HasCompleteAddress(string street, string houseNumber, string postalCode, string place)
    {
        return string.IsNullOrWhiteSpace(street) == false
            && string.IsNullOrWhiteSpace(houseNumber) == false
            && string.IsNullOrWhiteSpace(postalCode) == false
            && string.IsNullOrWhiteSpace(place) == false;
    }

    private static (string Street, string HouseNumber) GetStreetAndHouseNumber(PatientCsvRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Street) == false || string.IsNullOrWhiteSpace(record.HouseNumber) == false)
            return (record.Street, record.HouseNumber);

        return SplitStreetAndNumber(record.StreetAndNumber);
    }

    private static (string Street, string HouseNumber) SplitStreetAndNumber(string streetAndNumber)
    {
        string value = streetAndNumber.Trim();
        int splitIndex = value.LastIndexOf(' ');

        if (splitIndex <= 0 || splitIndex == value.Length - 1)
            return (value, "");

        return (value[..splitIndex], value[(splitIndex + 1)..]);
    }
}
