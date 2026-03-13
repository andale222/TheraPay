using TheraPay.Domain;

namespace TheraPay.Core;

public interface IPatientRepository
{
    Result Add(Patient patient);
    int Count();
    Patient GetPatient(int index);
    void Clear();
    IReadOnlyList<Patient> GetAll();
}
