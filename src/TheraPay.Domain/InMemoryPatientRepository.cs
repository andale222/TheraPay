namespace TheraPay.Domain;

public class InMemoryPatientRepository
{
    private readonly List<Patient> _patients = new List<Patient>();

    public Result Add(Patient patient)
    {
        if (PatientExists(patient.ID))
        {
            return new Result(false, $"Patient with ID {patient.ID} already exists.");
        }
        
        _patients.Add(patient);
        return new Result(true);
    }

    public int Count()
    {
        return _patients.Count;
    }

    public Patient GetPatient(int index)
    {
        return _patients[index];
    }

    private bool PatientExists(string id)
    {
        return _patients.Any(p => p.ID == id);
    }
}