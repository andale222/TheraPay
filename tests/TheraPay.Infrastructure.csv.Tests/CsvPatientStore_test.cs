using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvPatientStore_test
{
    [Fact]
    public void GivenNonExistingFile_LoadAll_ReturnsEmpty()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(TestPaths.DataFile("nonExistingPatients.csv"));

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.Empty(patients);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsPatients()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(TestPaths.DataFile("testLoadPatients.csv"));

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.NotEmpty(patients);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsCorrectPatients()
    {
        // Given
        var csvPatientStore = new CsvPatientStore(TestPaths.DataFile("testLoadPatients.csv"));

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
        Assert.All(patients, patient => Assert.True(patient.IsActive));
    }

    [Fact]
    public void GivenEmptyList_SaveAll_FileExists()
    {
        // Given
        var filePath = TestPaths.DataFile("testEmptySavePatients.csv");
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
        var filePath = TestPaths.DataFile("testRoundtripPatients.csv");
        var csvPatientStore = new CsvPatientStore(filePath);
        var secondPatient = new Patient("Second one", "Two", "2w");
        secondPatient.SetSalutation("Frau");
        secondPatient.SetDateOfBirth(new DateOnly(1991, 4, 12));
        secondPatient.SetInsuranceStatus(PatientInsuranceStatus.GKV);
        var thirdPatient = new Patient("Third", "Threee", "3");
        thirdPatient.SetSalutation("Divers");
        thirdPatient.SetDateOfBirth(new DateOnly(1985, 9, 30));
        thirdPatient.SetInsuranceStatus(PatientInsuranceStatus.Selbstzahler);
        thirdPatient.IsActive = false;
        var patients = new List<Patient>
        {
            new Patient("Firstt", "One", "1"),
            secondPatient,
            thirdPatient
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
        Assert.Equal(patients[1].Salutation, loadedPatients[1].Salutation);
        Assert.Equal(patients[1].FirstName, loadedPatients[1].FirstName);
        Assert.Equal(patients[1].LastName, loadedPatients[1].LastName);
        Assert.Equal(patients[1].DateOfBirth, loadedPatients[1].DateOfBirth);
        Assert.Equal(patients[1].InsuranceStatus, loadedPatients[1].InsuranceStatus);
        Assert.Equal(patients[2].ID, loadedPatients[2].ID);
        Assert.Equal(patients[2].Salutation, loadedPatients[2].Salutation);
        Assert.Equal(patients[2].FirstName, loadedPatients[2].FirstName);
        Assert.Equal(patients[2].LastName, loadedPatients[2].LastName);
        Assert.Equal(patients[2].DateOfBirth, loadedPatients[2].DateOfBirth);
        Assert.Equal(patients[2].InsuranceStatus, loadedPatients[2].InsuranceStatus);
        Assert.False(loadedPatients[2].IsActive);

        string savedCsv = File.ReadAllText(filePath);
        Assert.Contains("Salutation", savedCsv);
        Assert.Contains("DateOfBirth", savedCsv);
        Assert.Contains("IsActive", savedCsv);
        Assert.Contains("1991-04-12", savedCsv);
        Assert.Contains("False", savedCsv);

        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void GivenCsvWithIsActive_LoadAll_RestoresInactivePatients()
    {
        // Given
        var filePath = TestPaths.DataFile("testLoadActivePatients.csv");
        File.WriteAllText(
            filePath,
            "Id,FirstName,LastName,IsActive\nACTIVE,Ada,Active,True\nINACTIVE,Ida,Inactive,False\n");
        var csvPatientStore = new CsvPatientStore(filePath);

        // When
        var patients = csvPatientStore.LoadAll();

        // Then
        Assert.Equal(2, patients.Count);
        Assert.True(patients[0].IsActive);
        Assert.False(patients[1].IsActive);

        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }
}
