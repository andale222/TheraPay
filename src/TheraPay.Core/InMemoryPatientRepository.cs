namespace TheraPay.Core;

using TheraPay.Domain;

public class InMemoryPatientRepository : InMemoryRepositoryBase<Patient>, IPatientRepository
{
    protected override Result EntityExists(Patient entity)
    {
        bool exists = Items.Any(p => p.ID == entity.ID);
        if (exists)
            return new Result(exists, $"Patient with ID {entity.ID} already exists.");

        return new Result(false, $"Patient with ID {entity.ID} not found.");
    }
}
