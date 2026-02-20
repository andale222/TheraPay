using TheraPay.Domain;

namespace TheraPay.Core.Tests;


public class PatientOperations_test
{
    private Patient getFirstPatient( )
    {
        return new Patient("A", "J", "L5R");
    }
    private Patient getSecondPatient( )
    {
        return new Patient("second", "patient", "NR2");
    }
    [Fact]
    public void EmptyPatientRepository_AddPatient_Count1()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(getFirstPatient( ));

        // THEN
        Assert.Equal(1, patients.Count());
    }

    [Fact]
    public void EmptyPatientRepository_AddPatient_Count2()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(getFirstPatient( ));
        patients.Add(getSecondPatient( ));

        // THEN
        Assert.Equal(2, patients.Count());
    }

    

    [Fact]
    public void EmptyPatientRepository_AddPatient_FirstPatientEqualsAdded()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        Patient newPatient = getFirstPatient( );
        patients.Add(newPatient);

        // THEN
        Assert.Equal(patients.GetPatient(0).LastName, newPatient.LastName);
        Assert.Equal(patients.GetPatient(0).FirstName, newPatient.FirstName);
        Assert.Equal(patients.GetPatient(0).ID, newPatient.ID);
    }

    [Fact]
    public void EmptyPatientRepository_Add2Patients_SecondPatientEqualsAdded()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        patients.Add(getFirstPatient( ));
        Patient newPatient = getSecondPatient();
        patients.Add(newPatient);

        // THEN
        Assert.Equal(patients.GetPatient(1).LastName, newPatient.LastName);
        Assert.Equal(patients.GetPatient(1).FirstName, newPatient.FirstName);
        Assert.Equal(patients.GetPatient(1).ID, newPatient.ID);
    }

    [Fact]
    public void EmptyPatientRepository_Add2SimilarPatients_ThrowExceptionForSecond()
    {
        // GIVEN
        InMemoryPatientRepository patients = new InMemoryPatientRepository();

        // WHEN
        Result result = patients.Add(getFirstPatient( ));
        Result result2 = patients.Add(getFirstPatient( ));

        // THEN
        Assert.Equal(true, result.Ok);
        Assert.Equal(false, result2.Ok);
        // maybe
        Assert.Equal("Patient with ID L5R already exists.", result2.Error);
    }


}