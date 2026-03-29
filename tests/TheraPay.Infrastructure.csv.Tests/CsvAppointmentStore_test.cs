using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvAppointmentStore_test
{
    [Fact]
    public void GivenNonExistingFile_LoadAll_ReturnsEmpty()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(TestPaths.DataFile("nonExistingAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.Empty(appointments);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsAppointments()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(TestPaths.DataFile("testLoadAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.NotEmpty(appointments);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsCorrectAppointments()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(TestPaths.DataFile("testLoadAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.Equal(2, appointments.Count);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 5, 0), appointments[0].Date.ToUniversalTime());
        Assert.Equal(25, appointments[0].DurationInMinutes);
        Assert.Equal("Pat1", appointments[0].PatientID);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), appointments[0].Id);
        Assert.Equal(new DateTime(2026, 2, 28, 9, 0, 0), appointments[1].Date.ToUniversalTime());
        Assert.Equal(50, appointments[1].DurationInMinutes);
        Assert.Equal("Pat2", appointments[1].PatientID);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), appointments[1].Id);
        Assert.Equal(AppointmentStatus.Open, appointments[0].Status);
        Assert.Equal(AppointmentStatus.Billed, appointments[1].Status);
    }

    [Fact]
    public void GivenEmptyList_SaveAll_FileExists()
    {
        // Given
        var filePath = TestPaths.DataFile("testEmptySaveAppointments.csv");
        var csvAppointmentStore = new CsvAppointmentStore(filePath);
        var appointments = new List<Appointment>();

        // When
        csvAppointmentStore.SaveAll(new List<Appointment>());

        // Then
        Assert.True(File.Exists(filePath));
        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void GivenAppointmentList_SaveAllLoadAll_SavesAndLoadsAppointments()
    {
        // Given
        var filePath = TestPaths.DataFile("testRoundtripAppointments.csv");
        var csvAppointmentStore = new CsvAppointmentStore(filePath);
        var appointments = new List<Appointment>
        {
            new Appointment(DateTime.Now, "Pat1"),
            new Appointment(DateTime.Now.AddHours(1), "Pat2"),
            new Appointment(DateTime.Now.AddHours(2), "Pat3")
        };

        // When
        csvAppointmentStore.SaveAll(appointments);
        var loadedAppointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.Equal(3, loadedAppointments.Count);
        for (int i = 0; i < 3; ++i)
        {
            Assert.Equal(appointments[i].Date, loadedAppointments[i].Date);
            Assert.Equal(appointments[i].DurationInMinutes, loadedAppointments[i].DurationInMinutes);
            Assert.Equal(appointments[i].PatientID, loadedAppointments[i].PatientID);
            Assert.Equal(appointments[i].Id, loadedAppointments[i].Id);
        }

        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }
}
