using System.Runtime.CompilerServices;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvPatientStore_test
{

    private string getBaseDirectory()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        return dataDir;
    }

    /*
SaveAll_writes_patients_to_csv

LoadAll_reads_previously_saved_patients

SaveAll_and_LoadAll_roundtrip_preserves_values
    */
    [Fact]
    public void GivenNonExistingFile_LoadAll_ReturnsEmpty()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(Path.Combine(getBaseDirectory(), "nonExistingPatients.csv"));

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.Empty(patients);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsPatients()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(Path.Combine(getBaseDirectory(), "testLoadPatients.csv"));

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.NotEmpty(patients);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsCorrectPatients()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(Path.Combine(getBaseDirectory(), "testLoadPatients.csv"));

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.Equal(3, patients.Count);
        Assert.Equal("1", patients[0].ID);
        Assert.Equal("First", patients[0].FirstName);
        Assert.Equal("One", patients[0].LastName);
        Assert.Equal("2", patients[1].ID);
        Assert.Equal("Second one", patients[1].FirstName);
        Assert.Equal("Two", patients[1].LastName);
        Assert.Equal("3", patients[2].ID);
        Assert.Equal("Third", patients[2].FirstName);
        Assert.Equal("Three", patients[2].LastName);
    }

    [Fact]
    public void GivenEmptyList_SaveAll_FileExists()
    {
        // Given
        var filePath = Path.Combine(getBaseDirectory(), "testEmptySavePatients.csv");
        var csvPatientStore = new CsvPatientStore(filePath);
        var patients = new List<Patient>();

        // When
        csvPatientStore.SaveAll(new List<Patient>());

        // Then
        Assert.True(File.Exists(filePath));
        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void GivenPatientList_SaveAllLoadAll_SavesAndLoadsPatients()
    {
        // Given
        var filePath = Path.Combine(getBaseDirectory(), "testRoundtripPatients.csv");
        var csvPatientStore = new CsvPatientStore(filePath);
        var patients = new List<Patient>
        {
            new Patient("Firstt", "One", "1"),
            new Patient("Second one", "Two", "2w"),
            new Patient("Third", "Threee", "3")
        };

        // When
        csvPatientStore.SaveAll(patients);
        var loadedPatients = csvPatientStore.LoadAll();

        // Then
        Assert.Equal(3, loadedPatients.Count);
        Assert.Equal(patients[0].ID, loadedPatients[0].ID);
        Assert.Equal(patients[0].FirstName, loadedPatients[0].FirstName);
        Assert.Equal(patients[0].LastName, loadedPatients[0].LastName);
        Assert.Equal(patients[1].ID, loadedPatients[1].ID);
        Assert.Equal(patients[1].FirstName, loadedPatients[1].FirstName);
        Assert.Equal(patients[1].LastName, loadedPatients[1].LastName);
        Assert.Equal(patients[2].ID, loadedPatients[2].ID);
        Assert.Equal(patients[2].FirstName, loadedPatients[2].FirstName);
        Assert.Equal(patients[2].LastName, loadedPatients[2].LastName);
    }
}
