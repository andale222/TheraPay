using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;

public class PatientService_test
{

    [Fact]
    public void GivenPatientRepository_CreatePatientService_PatientServiceHasRepository()
    {
        // GIVEN
        InMemoryPatientRepository repository = new InMemoryPatientRepository();

        // WHEN
        PatientService service = new PatientService(repository);

        // THEN
        Assert.Equal(repository.GetAll(), service.ViewPatients());
    }

    [Fact]
    public void GivenPatientRepositoryWithPatients_AddPatient_GetAllReturnsPatients()
    {
        // GIVEN
        InMemoryPatientRepository repository = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        PatientService service = new PatientService(repository);

        // // WHEN
        var result = service.AddPatient("Friedrich", "Gauss", "FG7");
        IReadOnlyList<Patient> patients = service.ViewPatients();

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(3, patients.Count);
        Assert.Equal("Friedrich", patients[2].FirstName);
        Assert.Equal("Gauss", patients[2].LastName);
        Assert.Equal("FG7", patients[2].ID);
    }

    [Fact]
    public void GivenPatientRepositoryWithPatients_AddPatient1_ResultIsNotOk()
    {
        // GIVEN
        PatientService service = TestData.getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients();

        // WHEN
        var patient = TestData.Patient1();
        var result = service.AddPatient(patient.FirstName, patient.LastName, patient.ID);

        // THEN
        Assert.False(result.Ok);
    }

}