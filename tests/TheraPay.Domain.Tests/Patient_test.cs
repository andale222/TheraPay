using TheraPay.Domain;

namespace TheraPay.Domain.Tests;


public class Patient_test
{
    [Fact]
    public void GivenPatientData_CreatePatient_PatientHasCorrectValues()
    {
        // GIVEN
        string firstName = "A";
        string lastName = "J";
        string id = "L5R";

        // WHEN
        Patient patient = new Patient(firstName, lastName, id);

        // THEN
        Assert.Equal(firstName, patient.FirstName);
        Assert.Equal(lastName, patient.LastName);
        Assert.Equal(id, patient.ID);
    }

    [Fact]
    public void GivenPatientAddress_SetAddress_AddressFieldsComeFromAddress()
    {
        // GIVEN
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN
        patient.SetAddress("Teststraße", "81A", "12345", "Teststadt", "Deutschland", "2. OG");

        // THEN
        Assert.NotNull(patient.Address);
        Assert.Equal("Teststraße 81A", patient.Address.GetStreetNr());
        Assert.Equal("12345", patient.Address.PostalCode);
        Assert.Equal("Teststadt", patient.Address.City);
        Assert.Equal("2. OG", patient.Address.Additional);
    }

    [Fact]
    public void GivenPatient_CreatePatient_InsuranceStatusDefaultsToPrivat()
    {
        // WHEN
        Patient patient = new Patient("A", "J", "L5R");

        // THEN
        Assert.Equal(PatientInsuranceStatus.Privat, patient.InsuranceStatus);
    }

    [Fact]
    public void GivenPatient_SetSalutationAndDateOfBirth_ValuesAreUpdated()
    {
        // GIVEN
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN
        patient.SetSalutation("frau");
        patient.SetDateOfBirth(new DateOnly(1990, 5, 23));

        // THEN
        Assert.Equal("Frau", patient.Salutation);
        Assert.Equal(new DateOnly(1990, 5, 23), patient.DateOfBirth);
    }

    [Fact]
    public void GivenInvalidSalutation_SetSalutation_ThrowsArgumentException()
    {
        // GIVEN
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN / THEN
        Assert.Throws<ArgumentException>(() => patient.SetSalutation("Dr."));
    }

    [Fact]
    public void GivenPatient_SetInsuranceStatus_InsuranceStatusIsUpdated()
    {
        // GIVEN
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN
        patient.SetInsuranceStatus(PatientInsuranceStatus.GKV);

        // THEN
        Assert.Equal(PatientInsuranceStatus.GKV, patient.InsuranceStatus);
    }
}
