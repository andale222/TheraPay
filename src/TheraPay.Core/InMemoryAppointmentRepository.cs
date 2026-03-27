namespace TheraPay.Core;

using TheraPay.Domain;

public class InMemoryAppointmentRepository : InMemoryRepositoryBase<Appointment>, IAppointmentRepository
{
    protected override object GetEntityId(Appointment entity) => entity.Id;

    protected override Result EntityExists(Appointment entity)
    {
        bool exists = Items.Any(p => p.Id == entity.Id);
        if (exists)
            return new Result(exists, $"Appointment with ID {entity.Id} already exists.");

        return new Result(false, $"Appointment with ID {entity.Id} not found.");
    }
}
