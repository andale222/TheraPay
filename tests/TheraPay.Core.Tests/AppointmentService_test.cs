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
        Assert.Equal(1, result.Count);
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


}
