using TheraPay.Domain;

namespace TheraPay.Core;

public interface IAppointmentRepository
{
    void Add(Appointment appointment);
    int Count();
    Appointment GetAppointment(int index);
    IReadOnlyList<Appointment> GetAll();
}