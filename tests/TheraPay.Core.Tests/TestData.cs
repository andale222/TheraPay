namespace TheraPay.Core.Tests;

using TheraPay.Core;
using TheraPay.Domain;

public static class TestData
{

    public static Patient Patient1( ) => new Patient("A", "J", "L5R");
    public static Patient Patient2( ) => new Patient("second", "patient", "NR2");


    public static InMemoryPatientRepository getInMemoryPatientRepositoryWithTwoPatients()
    {
        InMemoryPatientRepository repository = new InMemoryPatientRepository();
        repository.Add(Patient1());
        repository.Add(Patient2());
        return repository;
    }
    public static PatientService getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients()
    {
        
        InMemoryPatientRepository repository = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        return new PatientService(repository);
    }
}