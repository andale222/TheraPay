using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvPracticeDataStore_test
{
    [Fact]
    public void GivenNonExistingFile_LoadAll_ReturnsDefaultPracticeData()
    {
        // Given
        var csvPracticeDataStore = new CsvPracticeDataStore(TestPaths.DataFile("nonExistingPracticeData.csv"));

        // When
        var practiceData = csvPracticeDataStore.Load();

        // Then
        Assert.NotNull(practiceData);
    }

    [Fact]
    public void GivenExistingFile_LoadAll_ReturnsCorrectPracticeData()
    {
        // Given
        var csvPracticeDataStore = new CsvPracticeDataStore(TestPaths.DataFile("testLoadPracticeData.csv"));

        // When
        var practiceData = csvPracticeDataStore.Load();

        // Then
        Assert.Equal("Privatpraxis test", practiceData.Name);
        Assert.Equal("Mara", practiceData.FirstNamePractitioner);
        Assert.Equal("Mustermann", practiceData.LastNamePractitioner);
        Assert.Equal("Teststraße 81", practiceData.StreetAndNumber);
        Assert.Equal("12345 Teststadt", practiceData.CityAndPostalCode);
        Assert.Equal("Deutschland", practiceData.Country);
        Assert.Equal("+49 12345678901", practiceData.PhoneNumber);
        Assert.Equal("DE12 3456 7890 1234 5678 9012", practiceData.IBAN);
        Assert.Equal("123456789", practiceData.TaxIdentificationNumber);
        Assert.Equal(21, practiceData.DefaultPaymentTermDays);
    }

    [Fact]
    public void GivenEmptyPracticeData_SaveAll_FileExists()
    {
        // Given
        var filePath = TestPaths.DataFile("testEmptySavePracticeData.csv");
        var csvPracticeDataStore = new CsvPracticeDataStore(filePath);
        var practiceData = new PracticeData();

        // When
        csvPracticeDataStore.Save(practiceData);

        // Then
        Assert.True(File.Exists(filePath));
        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void GivenPracticeData_SaveAllLoadAll_SavesAndLoadsPracticeData()
    {
        // Given
        var filePath = TestPaths.DataFile("testRoundtripPracticeData.csv");
        var csvPracticeDataStore = new CsvPracticeDataStore(filePath);
        var practiceData = new PracticeData
        {
            Name = "Test Practice",
            FirstNamePractitioner = "John",
            LastNamePractitioner = "Doe",
            StreetAndNumber = "123 Test Street",
            CityAndPostalCode = "45678 Test City",
            Country = "Test Country",
            PhoneNumber = "+1 234 567 8901",
            IBAN = "DE12 3456 7890 1234 5678 9012",
            TaxIdentificationNumber = "123456789",
            DefaultPaymentTermDays = 30
        };

        // When
        csvPracticeDataStore.Save(practiceData);
        var loadedPracticeData = csvPracticeDataStore.Load();

        // Then
        Assert.Equal(practiceData.Name, loadedPracticeData.Name);
        Assert.Equal(practiceData.FirstNamePractitioner, loadedPracticeData.FirstNamePractitioner);
        Assert.Equal(practiceData.LastNamePractitioner, loadedPracticeData.LastNamePractitioner);
        Assert.Equal(practiceData.StreetAndNumber, loadedPracticeData.StreetAndNumber);
        Assert.Equal(practiceData.CityAndPostalCode, loadedPracticeData.CityAndPostalCode);
        Assert.Equal(practiceData.Country, loadedPracticeData.Country);
        Assert.Equal(practiceData.PhoneNumber, loadedPracticeData.PhoneNumber);
        Assert.Equal(practiceData.IBAN, loadedPracticeData.IBAN);
        Assert.Equal(practiceData.TaxIdentificationNumber, loadedPracticeData.TaxIdentificationNumber);
        Assert.Equal(practiceData.DefaultPaymentTermDays, loadedPracticeData.DefaultPaymentTermDays);

        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }
}
