using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvDataPersistence_test
{

    private string getBaseDirectory()
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        return dataDir;
    }



    [Fact]
    public void GivenCsvPatientStoreAndPatientRepository_LoadInto_FillsPatientRepository()
    {
        // Arrange
        var dataDir = Path.Combine(getBaseDirectory(), "testLoadPatients.csv");
        var patientStore = new CsvPatientStore(dataDir);
        var dataPersistence = new CsvDataPersistence(patientStore);
        var patientRepository = new InMemoryPatientRepository();

        // Act
        var loadedPatients = patientStore.LoadAll();
        dataPersistence.LoadInto(patientRepository);

        // Assert
        var patients = patientRepository.GetAll().ToList();
        Assert.NotEmpty(patients);
        Assert.Equal(loadedPatients.Count(), patients.Count);
        foreach (var loadedPatient in loadedPatients)
        {
            Assert.Contains(patients, p => p.ID == loadedPatient.ID);
        }
    }

    [Fact]
    public void GivenCsvPatientStoreAndPatientRepository_SaveFrom_SavesPatientsToCsv()
    {
        // Given
        var dataDir = Path.Combine(getBaseDirectory(), "testSavePatients_DataPersistence.csv");
        var patientStore = new CsvPatientStore(dataDir);
        var dataPersistence = new CsvDataPersistence(patientStore);
        var patientRepository = new InMemoryPatientRepository();
        var patient1 = new Patient("John", "Doe", "1d3");
        var patient2 = new Patient("Jane", "Smith", "wed");
        patientRepository.Add(patient1);
        patientRepository.Add(patient2);

        // When
        dataPersistence.SaveFrom(patientRepository);

        // Then
        var loadedPatients = patientStore.LoadAll();
        Assert.NotEmpty(loadedPatients);
        Assert.Equal(2, loadedPatients.Count());
        Assert.Contains(loadedPatients, p => p.ID == patient1.ID);
        Assert.Contains(loadedPatients, p => p.ID == patient2.ID);
        File.Delete(dataDir);
        Assert.False(File.Exists(dataDir));
    }   

}