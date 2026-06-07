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
    public void GivenBillingNumbers_AddAppointment_AssignsBillingNumbers()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        var billingNumber = BillingNumberCatalog.FindByIdentifier("801a")!;

        // WHEN
        service.AddAppointment(
            new DateTime(2026, 1, 1, 14, 0, 0),
            "Pat1",
            60,
            [billingNumber]);

        // THEN
        var result = service.ViewAppointments();
        Assert.Single(result);
        Assert.Single(result[0].BillingNumbers);
        Assert.Equal(billingNumber, result[0].BillingNumbers[0]);
        Assert.Equal(billingNumber.Amount, result[0].TotalAmount);
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
    public void GivenExistingAppointment_UpdateAppointment_UpdatesAppointmentData()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "Pat1");
        appointment.SetDuration(60);
        repository.Add(appointment);
        var billingNumber = BillingNumberCatalog.FindByIdentifier("801a")!;

        // WHEN
        var result = service.UpdateAppointment(
            appointment.Id,
            new DateTime(2026, 1, 1, 16, 0, 0),
            "Pat2",
            30,
            [billingNumber]);

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(new DateTime(2026, 1, 1, 16, 0, 0), appointment.Date);
        Assert.Equal("Pat2", appointment.PatientID);
        Assert.Equal(30, appointment.DurationInMinutes);
        Assert.Single(appointment.BillingNumbers);
        Assert.Equal(billingNumber, appointment.BillingNumbers[0]);
    }

    [Fact]
    public void GivenExistingAppointment_UpdateAppointmentToOverlap_ReturnsWarning()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        var appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "Pat1");
        appointment1.SetDuration(60);
        var appointment2 = new Appointment(new DateTime(2026, 1, 1, 16, 0, 0), "Pat2");
        appointment2.SetDuration(60);
        repository.Add(appointment1);
        repository.Add(appointment2);

        // WHEN
        var result = service.UpdateAppointment(
            appointment2.Id,
            new DateTime(2026, 1, 1, 14, 30, 0),
            "Pat2",
            60);

        // THEN
        Assert.False(result.Ok);
        Assert.Equal("Overlapping appointment", result.Error);
        Assert.Equal(new DateTime(2026, 1, 1, 16, 0, 0), appointment2.Date);
    }

    [Fact]
    public void GivenExistingAppointment_DeleteAppointment_RemovesAppointment()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        AppointmentService service = new AppointmentService(repository);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "Pat1");
        repository.Add(appointment);

        // WHEN
        var result = service.DeleteAppointment(appointment.Id);

        // THEN
        Assert.True(result.Ok);
        Assert.Empty(service.ViewAppointments());
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
