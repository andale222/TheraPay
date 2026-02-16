using System.Reflection;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;


public class PatientOperations_test
{
    [Fact]
    public void EmptyPatientRepository_AddPatient_Count1()
    {
        // GIVEN
        var patients = new List<Patient>();

        // WHEN
        patients.Add(new Patient());

        // THEN
        Assert.Equal(1, patients.Count);
    }
}