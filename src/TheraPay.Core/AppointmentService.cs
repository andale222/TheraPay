namespace TheraPay.Core;

using TheraPay.Domain;

public class AppointmentService
{
    private readonly InMemoryAppointmentRepository _repository;
    public AppointmentService(InMemoryAppointmentRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Appointment> GetAll()
    {
        return _repository.GetAll();
    }
}