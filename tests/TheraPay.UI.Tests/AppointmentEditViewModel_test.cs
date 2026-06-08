namespace TheraPay.UI.Tests;

using TheraPay.Domain;
using TheraPay.UI.ViewModels;

public class AppointmentEditViewModel_test
{
    [Fact]
    public void Test1()
    {

    }

    [Fact]
    public void GivenPatient_PatientFieldsFromPatient_PopulatesFormFields()
    {
        // GIVEN
        Patient patient = new("Ada", "Lovelace", "AL1");
        patient.SetAddress("Imaginary Road", "42B", "12345", "London", "UK", "near the analytical engine");
        patient.SetEmail("ada@example.com");
        patient.SetPhoneNumber("+123");
        patient.SetICD10Diagnosis("F12");
        patient.SetInsuranceStatus(PatientInsuranceStatus.Kostenerstattung);
        patient.IsActive = false;

        // WHEN
        PatientFields fields = PatientFields.FromPatient(patient);

        // THEN
        Assert.Equal("AL1", fields.PatientID);
        Assert.Equal("Ada", fields.FirstName);
        Assert.Equal("Lovelace", fields.LastName);
        Assert.Equal("Imaginary Road", fields.Street);
        Assert.Equal("42B", fields.HouseNumber);
        Assert.Equal("12345", fields.PostalCode);
        Assert.Equal("London", fields.Place);
        Assert.Equal("UK", fields.Country);
        Assert.Equal("near the analytical engine", fields.AdditionalInfo);
        Assert.Equal("ada@example.com", fields.Email);
        Assert.Equal("+123", fields.PhoneNumber);
        Assert.Equal("F12", fields.Icd10Diagnosis);
        Assert.Equal("Kostenerstattung", fields.InsuranceStatus);
        Assert.False(fields.IsActive);
    }
}
