using TheraPay.Domain;

namespace TheraPay.Domain.Tests;

public class Appointment_test
{
    public static Appointment CreateAppointment() => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
    [Fact]
    public void GivenAppointmentData_CreateAppointment_AppointmentHasCorrectValues()
    {
        // GIVEN
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Patient patient = new Patient("A", "J", "L5R");

        // WHEN
        var appointment = new Appointment(date, patient.ID);

        // THEN
        Assert.Equal(date, appointment.Date);
        Assert.Equal(patient.ID, appointment.PatientID);
        Assert.NotEqual(Guid.Empty, appointment.Id);
    }
    [Fact]
    public void GivenAppointment_AddDuration_DurationHasCorrectValue()
    {
        // GIVEN
        var appointment = CreateAppointment();

        // WHEN
        appointment.SetDuration(50);

        // THEN
        Assert.Equal(50, appointment.DurationInMinutes);
    }
    [Fact]
    public void GivenAppointment_OverrideDuration_DurationHasCorrectValue()
    {
        // GIVEN
        var appointment = CreateAppointment();

        // WHEN
        appointment.SetDuration(50);
        appointment.SetDuration(10);

        // THEN
        Assert.Equal(10, appointment.DurationInMinutes);
    }
    [Fact]
    public void GivenAppointment_AddNegativeDuration_ThrowsException()
    {
        // GIVEN
        var appointment = CreateAppointment();

        // WHEN THEN
        Assert.Throws<ArgumentOutOfRangeException>(() => appointment.SetDuration(-10));

    }
    [Fact]
    public void GivenAppointment_AddTooLongDuration_ThrowsException()
    {
        // GIVEN
        var appointment = CreateAppointment();

        // WHEN THEN
        Assert.Throws<ArgumentOutOfRangeException>(() => appointment.SetDuration(25 * 60)); // 25 hours is too long
    }

}