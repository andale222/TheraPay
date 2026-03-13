namespace TheraPay.Core;

using TheraPay.Domain;

public class PatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public Result AddPatient(string firstName, string lastName, string id)
    {
        Patient patient = new Patient(firstName, lastName, id);
        Result result = _repository.Add(patient);
        return result;
    }

    public IReadOnlyList<Patient> ViewPatients()
    {
        return _repository.GetAll();
    }
}
