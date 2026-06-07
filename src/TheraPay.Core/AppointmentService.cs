using TheraPay.Domain;

namespace TheraPay.Core;

public class AppointmentService
{
    private readonly IAppointmentRepository _repository;
    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Appointment> ViewAppointments()
    {
        return _repository.GetAll();
    }

    public IReadOnlyList<Appointment> GetAppointmentsByDate(DateTime date)
    {
        return _repository
            .GetAll()
            .Where(appointment => appointment.Date.Date == date.Date)
            .ToList();
    }
    public IReadOnlyList<Appointment> GetNotBilledAppointmentsForPatient(string patientId)
    {
        var appointments = _repository.GetNonBilledAppointmentsOfPatient(patientId);

        return appointments;
    }

    public Result AddAppointment(
        DateTime date,
        string patientID,
        int durationInMinutes,
        IEnumerable<BillingNumber>? billingNumbers = null)
    {
        Appointment appointment = new Appointment(date, patientID);
        appointment.SetDuration(durationInMinutes);
        if (billingNumbers != null)
        {
            appointment.SetBillingNumbers(billingNumbers);
        }

        if (_repository.GetAll().Any(a => a.OverlapsWith(appointment)))
        {
            return new Result(false, "Overlapping appointment");
        }
        _repository.Add(appointment);

        return new Result(true,"");
    }
}
