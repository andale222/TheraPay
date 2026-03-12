namespace TheraPay.Core;

using TheraPay.Domain;

public class InMemoryAppointmentRepository : IAppointmentRepository
{

    private readonly List<Appointment> _appointments = new List<Appointment>();

    public void Add(Appointment appointment)
    {
        _appointments.Add(appointment);
    }

    public int Count()
    {
        return _appointments.Count;
    }

    public Appointment GetAppointment(int index)
    {
        return _appointments[index];
    }

    public IReadOnlyList<Appointment> GetAll()
    {
        return _appointments;
    }
}