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
        Assert.Equal(AppointmentStatus.Open, appointment.Status);
    }

    [Fact]
    public void GivenPersistedAppointmentData_RehydrateAppointment_AppointmentHasCorrectValues()
    {
        // GIVEN
        var id = Guid.Parse("89ec0878-12eb-42a3-9041-13a9d5f22f01");
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        const string patientId = "L5R";
        const int duration = 60;

        // WHEN
        var appointment = Appointment.Rehydrate(id, date, patientId, duration, AppointmentStatus.Billed);

        // THEN
        Assert.Equal(id, appointment.Id);
        Assert.Equal(date, appointment.Date);
        Assert.Equal(patientId, appointment.PatientID);
        Assert.Equal(duration, appointment.DurationInMinutes);
        Assert.Equal(AppointmentStatus.Billed, appointment.Status);
    }

    [Fact]
    public void GivenEmptyId_RehydrateAppointment_ThrowsArgumentException()
    {
        // WHEN THEN
        Assert.Throws<ArgumentException>(() =>
            Appointment.Rehydrate(Guid.Empty, new DateTime(2026, 1, 1, 14, 0, 0), "patientID", 60, AppointmentStatus.Open));
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
    [Fact]
    public void GivenAppointment_AddDuration_EndPropertyIsSetCorrectly()
    {
        // GIVEN
        var appointment = CreateAppointment();
        int durationInMinutes = 90;
        DateTime expectedEnd = appointment.Date.AddMinutes(durationInMinutes);

        // WHEN
        appointment.SetDuration(durationInMinutes);

        // THEN
        Assert.Equal(expectedEnd, appointment.End);

        // WHEN
        appointment.SetDuration(10);
        expectedEnd = appointment.Date.AddMinutes(10);

        // THEN
        Assert.Equal(expectedEnd, appointment.End);
    }
    [Fact]
    public void GivenTwoAppointments_OverlappingEnd_ReturnsTrue()
    {        // GIVEN
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 14, 30, 0), "patientID2");
        appointment1.SetDuration(60);
        appointment2.SetDuration(60);

        // WHEN
        bool overlaps = appointment1.OverlapsWith(appointment2);

        // THEN
        Assert.True(overlaps);
    }
    [Fact]
    public void GivenTwoAppointments_OverlappingStart_ReturnsTrue()
    {        // GIVEN
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 13, 30, 0), "patientID2");
        appointment1.SetDuration(60);
        appointment2.SetDuration(60);

        // WHEN
        bool overlaps = appointment1.OverlapsWith(appointment2);

        // THEN
        Assert.True(overlaps);
    }
    [Fact]
    public void GivenTwoAppointments_OverlappingInternal_ReturnsTrue()
    {        // GIVEN
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 14, 5, 0), "patientID2");
        appointment1.SetDuration(60);
        appointment2.SetDuration(30);

        // WHEN
        bool overlaps = appointment1.OverlapsWith(appointment2);

        // THEN
        Assert.True(overlaps);
    }

    [Fact]
    public void GivenTwoAppointments_NonOverlappingBefore_ReturnsFalse()
    {        // GIVEN
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 13, 0, 0), "patientID2");
        appointment1.SetDuration(60);
        appointment2.SetDuration(30);

        // WHEN
        bool overlaps = appointment1.OverlapsWith(appointment2);

        // THEN
        Assert.False(overlaps);
    }

    [Fact]
    public void GivenTwoAppointments_NonOverlappingAfter_ReturnsFalse()
    {        // GIVEN
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 15, 0, 0), "patientID2");
        appointment1.SetDuration(60);
        appointment2.SetDuration(30);

        // WHEN
        bool overlaps = appointment1.OverlapsWith(appointment2);

        // THEN
        Assert.False(overlaps);
    }
[Fact]
    public void GivenAppointment_SetBilled_AppointmentStatusIsBilled()
    {
        // GIVEN
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");

        // WHEN
        appointment.SetStatusToBilled( );

        // THEN
        Assert.Equal(AppointmentStatus.Billed, appointment.Status);
    }
}
