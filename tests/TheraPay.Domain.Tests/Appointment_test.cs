using TheraPay.Domain;

namespace TheraPay.Domain.Tests;

public class Appointment_test
{
    [Fact]
    public void GivenAppointmentData_CreateAppointment_AppointmentHasCorrectValues()
    {
        // GIVEN
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN
        var appointment = new Appointment(date, patient);

        // THEN
        Assert.Equal(date, appointment.Date);
        Assert.Equal(patient, appointment.Patient);
    }
}