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
    private static List<Appointment> MyGetAppointmentsOfPatient(string patientId, List<Appointment> list)
    {
        var appointmentsOfPatient = list.FindAll(x => x.PatientID == patientId);

        return appointmentsOfPatient;
    }
    private static List<Appointment> MyGetNonBilledAppointments(List<Appointment> list)
    {
        return list.FindAll(x => x.Status == AppointmentStatus.Open);
    }
    public IReadOnlyList<Appointment> GetAppointmentsOfPatient(string patientId)
    {
        return MyGetAppointmentsOfPatient(patientId, Items);
    }
    public IReadOnlyList<Appointment> GetNonBilledAppointmentsOfPatient(string patientId)
    {
        var appointmentsOfPatient = MyGetAppointmentsOfPatient(patientId, Items);
        return MyGetNonBilledAppointments(appointmentsOfPatient);
    }

}
