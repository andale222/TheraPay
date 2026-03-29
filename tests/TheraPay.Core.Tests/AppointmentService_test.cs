using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;

public class AppointmentService_test
{

    [Fact]
    public void GivenAppointmentRepository_CreateAppointmentService_AppointmentServiceHasRepository()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();

        // WHEN
        AppointmentService service = new AppointmentService(repository);

        // THEN
        Assert.Equal(repository.GetAll(), service.ViewAppointments());
    }

    [Fact]
    public void GivenEmptyAppointmentRepository_AddAppointment_ReturnListWithAddedAppointment()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        Appointment appointment = TestData.Appointment1();
        appointment.SetDuration(60);

        // WHEN
        service.AddAppointment(appointment.Date, appointment.PatientID, appointment.DurationInMinutes);

        // THEN
        var result = service.ViewAppointments();
        Assert.Single(result);
        Assert.Equal(appointment.Date, result[0].Date);
        Assert.Equal(appointment.PatientID, result[0].PatientID);
        Assert.Equal(appointment.End, result[0].End);
    }
    [Fact]
    public void GivenEmptyAppointmentRepository_AddTwoAppointments_ReturnListWithTwoAppointments()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        Appointment appointment1 = TestData.Appointment1();
        Appointment appointment2 = TestData.Appointment2();

        // WHEN
        var result1 = service.AddAppointment(appointment1.Date, appointment1.PatientID, appointment1.DurationInMinutes);
        var result2 = service.AddAppointment(appointment2.Date, appointment2.PatientID, appointment2.DurationInMinutes);

        // THEN
        var result = service.ViewAppointments();
        Assert.Equal(2, result.Count);
        Assert.True(result1.Ok);
        Assert.True(result2.Ok);
    }
    [Fact]
    public void GivenAppointmentRepositoryWithTwoAppointments_AddOverlappingAppointment_ReturnWarning()
    {
        // GIVEN
        var service = TestData.getAppointmentServiceWithInMemoryAppointmentRepositoryWithTwoAppointments();
        Appointment appointment1 = TestData.Appointment1();

        // WHEN
        Result result = service.AddAppointment(appointment1.Date.AddMinutes(5), appointment1.PatientID, appointment1.DurationInMinutes);

        // THEN

        Assert.False(result.Ok);
        Assert.Equal("Overlapping appointment", result.Error);
    }

    [Fact]
    public void GivenRepositoryWithTwoAppointments_GetAppointmentsByDate_ReturnsOnlyAppointmentsFromThatDay()
    {
        // GIVEN
        var service = TestData.getAppointmentServiceWithInMemoryAppointmentRepositoryWithTwoAppointments();
        var targetDate = new DateTime(2026, 1, 1);

        // WHEN
        var result = service.GetAppointmentsByDate(targetDate);

        // THEN
        Assert.Single(result);
        Assert.Equal(targetDate, result[0].Date.Date);
    }

    [Fact]
    public void GivenRepositoryWithAppointmentsOnSameDayDifferentTimes_GetAppointmentsByDate_ReturnsBothAppointments()
    {
        // GIVEN
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var targetDate = new DateTime(2026, 1, 1);

        service.AddAppointment(new DateTime(2026, 1, 1, 9, 0, 0), "P1", 30);
        service.AddAppointment(new DateTime(2026, 1, 1, 15, 0, 0), "P2", 30);
        service.AddAppointment(new DateTime(2026, 1, 2, 10, 0, 0), "P3", 30);

        // WHEN
        var result = service.GetAppointmentsByDate(targetDate);

        // THEN
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(targetDate, a.Date.Date));
    }

    [Fact]
    public void GivenRepositoryWithTwoAppointments_GetAppointmentsByDateForMissingDay_ReturnsEmptyList()
    {
        // GIVEN
        var service = TestData.getAppointmentServiceWithInMemoryAppointmentRepositoryWithTwoAppointments();

        // WHEN
        var result = service.GetAppointmentsByDate(new DateTime(2026, 1, 2));

        // THEN
        Assert.Empty(result);
    }

}
