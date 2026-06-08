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

    public Appointment GetAppointmentById(Guid appointmentId)
    {
        return _repository.GetById(appointmentId);
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

    public Result UpdateAppointment(
        Guid appointmentId,
        DateTime date,
        string patientID,
        int durationInMinutes,
        IEnumerable<BillingNumber>? billingNumbers = null)
    {
        try
        {
            if (_repository.GetIndexById(appointmentId) < 0)
            {
                return new Result(false, $"Appointment with ID {appointmentId} not found.");
            }

            var existingAppointment = _repository.GetById(appointmentId);
            var updatedBillingNumbers = billingNumbers?.ToList() ?? existingAppointment.BillingNumbers.ToList();
            var updatedAppointment = Appointment.Rehydrate(
                appointmentId,
                date,
                patientID,
                durationInMinutes,
                existingAppointment.Status,
                updatedBillingNumbers);

            if (_repository.GetAll().Any(a => a.Id != appointmentId && a.OverlapsWith(updatedAppointment)))
            {
                return new Result(false, "Overlapping appointment");
            }

            existingAppointment.UpdateDetails(date, patientID, durationInMinutes, updatedBillingNumbers);
            return new Result(true, "");
        }
        catch (Exception ex)
        {
            return new Result(false, ex.Message);
        }
    }

    public Result DeleteAppointment(Guid appointmentId)
    {
        return _repository.RemoveById(appointmentId);
    }
}
