using System.Security.Cryptography;
using System.Text;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvFileEncryption_test
{
    private static readonly AesGcmCsvFileEncryptionOptions FastEncryptionOptions = new()
    {
        Pbkdf2Iterations = 10
    };

    [Fact]
    public void GivenEncryptedPatientStore_SaveAllLoadAll_DoesNotPersistPlaintext()
    {
        var filePath = TempFile("patients.enc");
        var encryption = CreateEncryption();
        var patients = new List<Patient>
        {
            new Patient("Alice", "Secure", "pat-1")
        };

        try
        {
            var store = new CsvPatientStore(filePath, encryption);

            store.SaveAll(patients);
            var persistedText = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));
            var loadedPatients = store.LoadAll();

            Assert.DoesNotContain("Alice", persistedText);
            Assert.DoesNotContain("FirstName", persistedText);
            Assert.Single(loadedPatients);
            Assert.Equal("pat-1", loadedPatients[0].ID);
            Assert.Equal("Alice", loadedPatients[0].FirstName);
            Assert.Equal("Secure", loadedPatients[0].LastName);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public void GivenEncryptedPracticeDataStore_SaveLoad_DoesNotPersistPlaintext()
    {
        var filePath = TempFile("practice.enc");
        var encryption = CreateEncryption();
        var practiceData = new PracticeData
        {
            Name = "Praxis Geheim",
            IBAN = "DE12 3456 7890 1234 5678 9012",
            DefaultPaymentTermDays = 14
        };

        try
        {
            var store = new CsvPracticeDataStore(filePath, encryption);

            store.Save(practiceData);
            var persistedText = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));
            var loadedPracticeData = store.Load();

            Assert.DoesNotContain("Praxis Geheim", persistedText);
            Assert.DoesNotContain("IBAN", persistedText);
            Assert.Equal(practiceData.Name, loadedPracticeData.Name);
            Assert.Equal(practiceData.IBAN, loadedPracticeData.IBAN);
            Assert.Equal(practiceData.DefaultPaymentTermDays, loadedPracticeData.DefaultPaymentTermDays);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public void GivenEncryptedFile_DecryptFile_WritesPlaintextCsvForDebugging()
    {
        var encryptedPath = TempFile("patients.enc");
        var plaintextPath = TempFile("patients.csv");
        var encryption = CreateEncryption();

        try
        {
            var store = new CsvPatientStore(encryptedPath, encryption);
            store.SaveAll(new[] { new Patient("Debug", "Patient", "debug-1") });

            encryption.DecryptFile(encryptedPath, plaintextPath);
            var plaintextCsv = File.ReadAllText(plaintextPath);

            Assert.Contains("FirstName", plaintextCsv);
            Assert.Contains("Debug", plaintextCsv);
            Assert.Contains("Patient", plaintextCsv);
        }
        finally
        {
            DeleteIfExists(encryptedPath);
            DeleteIfExists(plaintextPath);
        }
    }

    [Fact]
    public void GivenPlaintextCsv_EncryptFile_WritesEncryptedFileLoadableByCsvStore()
    {
        var plaintextPath = TempFile("patients.csv");
        var encryptedPath = TempFile("patients.enc");
        var encryption = CreateEncryption();

        try
        {
            var plaintextStore = new CsvPatientStore(plaintextPath);
            plaintextStore.SaveAll(new[] { new Patient("Repair", "Patient", "repair-1") });

            encryption.EncryptFile(plaintextPath, encryptedPath);
            var persistedText = Encoding.UTF8.GetString(File.ReadAllBytes(encryptedPath));
            var loadedPatients = new CsvPatientStore(encryptedPath, encryption).LoadAll();

            Assert.DoesNotContain("Repair", persistedText);
            Assert.DoesNotContain("FirstName", persistedText);
            Assert.Single(loadedPatients);
            Assert.Equal("repair-1", loadedPatients[0].ID);
            Assert.Equal("Repair", loadedPatients[0].FirstName);
        }
        finally
        {
            DeleteIfExists(plaintextPath);
            DeleteIfExists(encryptedPath);
        }
    }

    [Fact]
    public void GivenWrongPassword_LoadAll_ThrowsCryptographicException()
    {
        var filePath = TempFile("patients.enc");
        var encryption = CreateEncryption();
        var wrongEncryption = new AesGcmCsvFileEncryption("wrong-password", FastEncryptionOptions);

        try
        {
            new CsvPatientStore(filePath, encryption).SaveAll(new[] { new Patient("Alice", "Secure", "pat-1") });
            var storeWithWrongPassword = new CsvPatientStore(filePath, wrongEncryption);

            Assert.ThrowsAny<CryptographicException>(() => storeWithWrongPassword.LoadAll());
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public void GivenMockEncryption_SaveAll_WritesPlaintextCsv()
    {
        var filePath = TempFile("patients.csv");

        try
        {
            var store = new CsvPatientStore(filePath, MockCsvFileEncryption.Instance);

            store.SaveAll(new[] { new Patient("Plain", "Patient", "plain-1") });
            var persistedText = File.ReadAllText(filePath);

            Assert.Contains("FirstName", persistedText);
            Assert.Contains("Plain", persistedText);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    private static AesGcmCsvFileEncryption CreateEncryption()
    {
        return new AesGcmCsvFileEncryption("correct horse battery staple", FastEncryptionOptions);
    }

    private static string TempFile(string suffix)
    {
        return Path.Combine(Path.GetTempPath(), $"TheraPay_{Guid.NewGuid():N}_{suffix}");
    }

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
