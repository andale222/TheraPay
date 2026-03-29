using TheraPay.Domain;

namespace TheraPay.Core;

public interface IAppointmentRepository : IRepository<Appointment>
{
    public IReadOnlyList<Appointment> GetAppointmentsOfPatient(string patientId);
    public IReadOnlyList<Appointment> GetNonBilledAppointmentsOfPatient(string patientId);
}