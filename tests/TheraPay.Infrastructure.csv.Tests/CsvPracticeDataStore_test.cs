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
        Assert.Equal("Teststraße", practiceData.Street);
        Assert.Equal("81", practiceData.HouseNumber);
        Assert.Equal("12345", practiceData.PostalCode);
        Assert.Equal("Teststadt", practiceData.City);
        Assert.Equal("Deutschland", practiceData.Country);
        Assert.Equal("2. OG", practiceData.AddressAdditional);
        Assert.Equal("+49 12345678901", practiceData.PhoneNumber);
        Assert.Equal("DE12 3456 7890 1234 5678 9012", practiceData.IBAN);
        Assert.Equal("11112222", practiceData.BLZ);
        Assert.Equal("Testbank", practiceData.BankName);
        Assert.Equal("Therapie Januar", practiceData.Subject);
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
            Street = "Test Street",
            HouseNumber = "123",
            PostalCode = "45678",
            City = "Test City",
            Country = "Test Country",
            AddressAdditional = "Floor 3",
            PhoneNumber = "+1 234 567 8901",
            IBAN = "DE12 3456 7890 1234 5678 9012",
            BLZ = "99887766",
            BankName = "Example Bank",
            Subject = "Invoice Payment",
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
        Assert.Equal(practiceData.Street, loadedPracticeData.Street);
        Assert.Equal(practiceData.HouseNumber, loadedPracticeData.HouseNumber);
        Assert.Equal(practiceData.PostalCode, loadedPracticeData.PostalCode);
        Assert.Equal(practiceData.City, loadedPracticeData.City);
        Assert.Equal(practiceData.Country, loadedPracticeData.Country);
        Assert.Equal(practiceData.AddressAdditional, loadedPracticeData.AddressAdditional);
        Assert.Equal(practiceData.PhoneNumber, loadedPracticeData.PhoneNumber);
        Assert.Equal(practiceData.IBAN, loadedPracticeData.IBAN);
        Assert.Equal(practiceData.BLZ, loadedPracticeData.BLZ);
        Assert.Equal(practiceData.BankName, loadedPracticeData.BankName);
        Assert.Equal(practiceData.Subject, loadedPracticeData.Subject);
        Assert.Equal(practiceData.TaxIdentificationNumber, loadedPracticeData.TaxIdentificationNumber);
        Assert.Equal(practiceData.DefaultPaymentTermDays, loadedPracticeData.DefaultPaymentTermDays);

        File.Delete(filePath);
        Assert.False(File.Exists(filePath));
    }
}
