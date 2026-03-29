using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;


public class PatientOperations_test
{
    [Fact]
    public void EmptyPatientRepository_AddPatient_Count1()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(TestData.Patient1( ));

        // THEN
        Assert.Equal(1, patients.Count());
    }

    [Fact]
    public void EmptyPatientRepository_AddPatient_Count2()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(TestData.Patient1( ));
        patients.Add(TestData.Patient2( ));

        // THEN
        Assert.Equal(2, patients.Count());
    }

    

    [Fact]
    public void EmptyPatientRepository_AddPatient_FirstPatientEqualsAdded()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        Patient newPatient = TestData.Patient1( );
        patients.Add(newPatient);

        // THEN
        Assert.Equal(patients.GetByIndex(0), newPatient);
    }

    [Fact]
    public void EmptyPatientRepository_Add2Patients_SecondPatientEqualsAdded()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(TestData.Patient1( ));
        Patient newPatient = TestData.Patient2();
        patients.Add(newPatient);

        // THEN
        Assert.Equal(patients.GetByIndex(1), newPatient);
    }

    [Fact]
    public void EmptyPatientRepository_Add2SimilarPatients_ReturnFailedResultForSecond()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        Result result = patients.Add(TestData.Patient1( ));
        Result result2 = patients.Add(TestData.Patient1( ));

        // THEN
        Assert.True(result.Ok);
        Assert.False(result2.Ok);
        // maybe
        Assert.Equal("Patient with ID L5R already exists.", result2.Error);
    }

    [Fact]
    public void EmptyPatientRepository_Add2Patients_GetAll_ReturnsListWith2Patients()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(TestData.Patient1( ));
        patients.Add(TestData.Patient2( ));
        IReadOnlyList<Patient> allPatients = patients.GetAll();

        // THEN
        Assert.Equal(2, allPatients.Count);
        Assert.Equal(TestData.Patient1( ).ID, allPatients[0].ID);
        Assert.Equal(TestData.Patient2( ).ID, allPatients[1].ID);
    }

    [Fact]
    public void GivenPatientRepositoryWithTwoPatients_GetByIdAndIndex_ReturnsCorrectPatient()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();
        var patient1 = TestData.Patient1();
        var patient2 = TestData.Patient2();
        patients.Add(patient1);
        patients.Add(patient2);

        // WHEN
        var byId = patients.GetById(patient2.ID);
        var index = patients.GetIndexById(patient2.ID);

        // THEN
        Assert.Equal(patient2, byId);
        Assert.Equal(1, index);
    }

    [Fact]
    public void EmptyPatientRepository_GetByIdUnknown_ReturnsMinusOneAndThrows()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        var index = patients.GetIndexById("unknown");

        // THEN
        Assert.Equal(-1, index);
        Assert.Throws<KeyNotFoundException>(() => patients.GetById("unknown"));
    }

}
