namespace TheraPay.Core;

using TheraPay.Domain;

public class AppointmentService
{
    private readonly InMemoryAppointmentRepository _repository;
    public AppointmentService(InMemoryAppointmentRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Appointment> ViewAppointments()
    {
        return _repository.GetAll();
    }

    public Result AddAppointment(DateTime date, string patientID, int durationInMinutes)
    {
        Appointment appointment = new Appointment(date, patientID);
        appointment.SetDuration(durationInMinutes);

        if (_repository.GetAll().Any(a => a.OverlapsWith(appointment)))
        {
            return new Result(false, "Overlapping appointment");
        }
        _repository.Add(appointment);

        return new Result(true,"");
    }
}