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
}