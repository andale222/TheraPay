using System.Runtime.CompilerServices;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvAppointmentStore_test
{

    private string getBaseDirectory()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        return dataDir;
    }



    [Fact]
    public void GivenNonExistingFile_LoadAll_ReturnsEmpty()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(Path.Combine(getBaseDirectory(), "nonExistingAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.Empty(appointments);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsAppointments()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(Path.Combine(getBaseDirectory(), "testLoadAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.NotEmpty(appointments);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsCorrectAppointments()
    {
        // Given
        var csvAppointmentStore = new CsvAppointmentStore(Path.Combine(getBaseDirectory(), "testLoadAppointments.csv"));

        // When
        var appointments = csvAppointmentStore.LoadAll();

        // Then
        Assert.Equal(2, appointments.Count);
        Assert.Equal(new DateTime(2026,1,1,12,5,0).AddHours(1), appointments[0].Date);
        Assert.Equal(25, appointments[0].DurationInMinutes);
        Assert.Equal("Pat1", appointments[0].PatientID);
        Assert.Equal(new DateTime(2026,2,28,9,0,0).AddHours(0), appointments[1].Date);
        Assert.Equal(50, appointments[1].DurationInMinutes);
        Assert.Equal("Pat2", appointments[1].PatientID);
    }

    [Fact]
    public void GivenEmptyList_SaveAll_FileExists()
    {
        // Given
        var filePath = Path.Combine(getBaseDirectory(), "testEmptySaveAppointments.csv");
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
        var filePath = Path.Combine(getBaseDirectory(), "testRoundtripAppointments.csv");
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
        Assert.Equal(appointments[0].Date, loadedAppointments[0].Date);
        Assert.Equal(appointments[0].DurationInMinutes, loadedAppointments[0].DurationInMinutes);
        Assert.Equal(appointments[0].PatientID, loadedAppointments[0].PatientID);
        Assert.Equal(appointments[1].Date, loadedAppointments[1].Date);
        Assert.Equal(appointments[1].DurationInMinutes, loadedAppointments[1].DurationInMinutes);
        Assert.Equal(appointments[1].PatientID, loadedAppointments[1].PatientID);
        Assert.Equal(appointments[2].Date, loadedAppointments[2].Date);
        Assert.Equal(appointments[2].DurationInMinutes, loadedAppointments[2].DurationInMinutes);
        Assert.Equal(appointments[2].PatientID, loadedAppointments[2].PatientID);
    }
}
