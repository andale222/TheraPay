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
        Assert.Equal(appointment, repository.GetAppointment(0));
    }
}