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
        Assert.Equal(repository.GetAll(), service.GetAll());
    }

}