using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;


public class InMemoryAppointmentRepository_test
{
    [Fact]
    public void Given_CreateInMemoryAppointmentRepository_RepositoryIsEmpty()
    {
        // GIVEN

        // WHEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();

        // THEN
        Assert.NotNull(repository);
        Assert.Equal(0, repository.Count());
    }

    [Fact]
    public void GivenEmptyInMemoryAppointmentRepository_AddAppointment_CountIsOne()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        Appointment appointment = TestData.Appointment1();

        // WHEN
        repository.Add(appointment);

        // THEN
        Assert.Equal(1, repository.Count());
        Assert.Equal(appointment, repository.GetByIndex(0));
    }

    [Fact]
    public void GivenEmptyInMemoryAppointmentRepository_AddTwoAppointment_CountIsTwo()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        Appointment appointment1 = TestData.Appointment1();
        Appointment appointment2 = TestData.Appointment2();

        // WHEN
        repository.Add(appointment1);
        repository.Add(appointment2);

        // THEN
        Assert.Equal(2, repository.Count());
        Assert.Equal(appointment1, repository.GetByIndex(0));
        Assert.Equal(appointment2, repository.GetByIndex(1));
    }

    [Fact]
    public void GivenInMemoryAppointmentRepository_GetAll_ReturnsAllAppointments()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        Appointment appointment1 = TestData.Appointment1();
        Appointment appointment2 = TestData.Appointment2();

        // WHEN
        repository.Add(appointment1);
        repository.Add(appointment2);
        var allAppointments = repository.GetAll();

        // THEN
        Assert.Equal(2, allAppointments.Count());
    }

    [Fact]
    public void GivenInMemoryAppointmentRepository_GetByIdAndIndex_ReturnsMatchingAppointment()
    {
        // GIVEN
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        Appointment appointment1 = TestData.Appointment1();
        Appointment appointment2 = TestData.Appointment2();
        repository.Add(appointment1);
        repository.Add(appointment2);

        // WHEN
        var byId = repository.GetById(appointment2.Id);
        var index = repository.GetIndexById(appointment2.Id);

        // THEN
        Assert.Equal(appointment2, byId);
        Assert.Equal(1, index);
    }

}
