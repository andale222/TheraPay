using TheraPay.Core;
using TheraPay.UI.Services;
using TheraPay.UI.State;

namespace TheraPay.UI.Tests;

public class ProjectPersistenceServiceConversion_test
{
    [Fact]
    public void GivenPlaintextProjectFiles_EncryptThenDecrypt_WritesRoundtripFiles()
    {
        var sourceDirectory = TempDirectory();
        var encryptedDirectory = TempDirectory();
        var decryptedDirectory = TempDirectory();

        try
        {
            var patientPath = WriteFile(sourceDirectory, "patients.csv", "Id,FirstName\npat-1,Alice\n");
            var appointmentPath = WriteFile(sourceDirectory, "appointments.csv", "Id,PatientId\napt-1,pat-1\n");
            var invoicePath = WriteFile(sourceDirectory, "invoices.csv", "Id,Status\ninv-1,Draft\n");
            var practicePath = WriteFile(sourceDirectory, "practice.csv", "Name,IBAN\nPraxis,DE00\n");
            var service = CreateService();

            var encryptResult = service.EncryptProjectFiles(
                patientPath,
                appointmentPath,
                practicePath,
                invoicePath,
                encryptedDirectory,
                "conversion-password");
            var encryptedPatientPath = Path.Combine(encryptedDirectory, "patients.csv");

            var decryptResult = service.DecryptProjectFiles(
                encryptedPatientPath,
                Path.Combine(encryptedDirectory, "appointments.csv"),
                Path.Combine(encryptedDirectory, "practice.csv"),
                Path.Combine(encryptedDirectory, "invoices.csv"),
                decryptedDirectory,
                "conversion-password");

            Assert.True(encryptResult.Ok, encryptResult.Error);
            Assert.True(decryptResult.Ok, decryptResult.Error);
            Assert.DoesNotContain("Alice", File.ReadAllText(encryptedPatientPath));
            Assert.Equal(File.ReadAllText(patientPath), File.ReadAllText(Path.Combine(decryptedDirectory, "patients.csv")));
            Assert.Equal(File.ReadAllText(appointmentPath), File.ReadAllText(Path.Combine(decryptedDirectory, "appointments.csv")));
            Assert.Equal(File.ReadAllText(invoicePath), File.ReadAllText(Path.Combine(decryptedDirectory, "invoices.csv")));
            Assert.Equal(File.ReadAllText(practicePath), File.ReadAllText(Path.Combine(decryptedDirectory, "practice.csv")));
        }
        finally
        {
            DeleteDirectory(sourceDirectory);
            DeleteDirectory(encryptedDirectory);
            DeleteDirectory(decryptedDirectory);
        }
    }

    [Fact]
    public void GivenMissingConversionPassword_EncryptProjectFiles_ReturnsError()
    {
        var service = CreateService();

        var result = service.EncryptProjectFiles(
            "patients.csv",
            "appointments.csv",
            "practice.csv",
            "",
            TempDirectory(),
            "");

        Assert.False(result.Ok);
        Assert.Contains("Passwort", result.Error);
    }

    private static ProjectPersistenceService CreateService()
    {
        return new ProjectPersistenceService(
            new InMemoryPatientRepository(),
            new InMemoryAppointmentRepository(),
            new InMemoryInvoiceRepository(),
            new ProjectSession());
    }

    private static string TempDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "TheraPayConversion_" + Guid.NewGuid().ToString("N"));
    }

    private static string WriteFile(string directory, string fileName, string content)
    {
        Directory.CreateDirectory(directory);
        string filePath = Path.Combine(directory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private static void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
