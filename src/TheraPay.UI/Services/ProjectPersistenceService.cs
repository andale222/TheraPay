using System;
using System.Collections.Generic;
using System.IO;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.Infrastructure.csv;
using TheraPay.Infrastructure.Encryption;
using TheraPay.UI.State;

namespace TheraPay.UI.Services;

public sealed class ProjectPersistenceService
{
    private const int RequiredDatabaseFileCount = 3;

    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ProjectSession _session;
    private IFileEncryption _fileEncryption = DummyFileEncryption.Instance;

    public ProjectPersistenceService(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository,
        ProjectSession session)
    {
        _patientRepository = patientRepository;
        _appointmentRepository = appointmentRepository;
        _invoiceRepository = invoiceRepository;
        _session = session;
    }

    public Result LoadProject(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath = "",
        IFileEncryption? fileEncryption = null)
    {
        if (string.IsNullOrWhiteSpace(patientListPath))
        {
            return new Result(false, "Bitte gib mindestens einen Pfad zur Patientenliste an.");
        }

        if (string.IsNullOrWhiteSpace(appointmentListPath))
        {
            return new Result(false, "Bitte gib mindestens einen Pfad zur Terminliste an.");
        }

        if (string.IsNullOrWhiteSpace(practiceDataPath))
        {
            return new Result(false, "Bitte gib mindestens einen Pfad zu den Praxisdaten an.");
        }

        try
        {
            _fileEncryption = fileEncryption ?? DummyFileEncryption.Instance;
            _session.SetPatientListPath(patientListPath);
            _session.SetAppointmentListPath(appointmentListPath);
            _session.SetInvoiceListPath(ResolveInvoiceListPath(invoiceListPath, patientListPath, appointmentListPath, practiceDataPath));
            _session.SetPracticeDataPath(practiceDataPath);
            _patientRepository.Clear();
            _appointmentRepository.Clear();
            _invoiceRepository.Clear();
            CreatePersistence().LoadInto(_patientRepository, _appointmentRepository, _invoiceRepository);
            _session.SetPracticeData(CreatePracticeDataStore().Load() ?? new PracticeData());
            _session.MarkSaved();
            return new Result(true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"Laden fehlgeschlagen: {ex.Message}");
        }
    }

    public void StartEmptyProject(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath = "",
        IFileEncryption? fileEncryption = null)
    {
        _fileEncryption = fileEncryption ?? DummyFileEncryption.Instance;
        _patientRepository.Clear();
        _appointmentRepository.Clear();
        _invoiceRepository.Clear();

        if (!string.IsNullOrWhiteSpace(patientListPath))
        {
            _session.SetPatientListPath(patientListPath);
        }

        if (!string.IsNullOrWhiteSpace(appointmentListPath))
        {
            _session.SetAppointmentListPath(appointmentListPath);
        }

        _session.SetInvoiceListPath(ResolveInvoiceListPath(invoiceListPath, patientListPath, appointmentListPath, practiceDataPath));

        if (!string.IsNullOrWhiteSpace(practiceDataPath))
        {
            _session.SetPracticeDataPath(practiceDataPath);
        }

        _session.MarkSaved();
    }

    public Result SaveProject()
    {
        if (!_session.HasPatientListPath || !_session.HasAppointmentListPath || !_session.HasInvoiceListPath || !_session.HasPracticeDataPath)
        {
            return new Result(false, "Kein Speicherpfad gesetzt. Bitte zuerst ein Projekt laden oder im Startscreen einen Projektpfad angeben.");
        }

        try
        {
            CreatePersistence().SaveFrom(_patientRepository, _appointmentRepository, _invoiceRepository);
            CreatePracticeDataStore().Save(_session.PracticeData);
            _session.MarkSaved();
            return new Result(true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"Speichern fehlgeschlagen: {ex.Message}");
        }
    }

    public Result EncryptProjectFiles(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath,
        string outputDirectory,
        string password)
    {
        if (!TryCreateConversionEncryption(outputDirectory, password, out var encryption, out var error))
            return new Result(false, error);

        return ConvertProjectFiles(
            patientListPath,
            appointmentListPath,
            practiceDataPath,
            invoiceListPath,
            outputDirectory,
            source => encryption.EncryptFile(source.SourcePath, source.TargetPath),
            "Verschluesselung");
    }

    public Result DecryptProjectFiles(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath,
        string outputDirectory,
        string password)
    {
        if (!TryCreateConversionEncryption(outputDirectory, password, out var encryption, out var error))
            return new Result(false, error);

        return ConvertProjectFiles(
            patientListPath,
            appointmentListPath,
            practiceDataPath,
            invoiceListPath,
            outputDirectory,
            source => encryption.DecryptFile(source.SourcePath, source.TargetPath),
            "Entschluesselung");
    }

    private IDataPersistence CreatePersistence()
    {
        return new CsvDataPersistence(
            new CsvPatientStore(_session.PatientListPath, _fileEncryption),
            new CsvAppointmentStore(_session.AppointmentListPath, _fileEncryption),
            new CsvInvoiceStore(_session.InvoiceListPath, _fileEncryption));
    }

    private IPracticeDataStore CreatePracticeDataStore()
    {
        return new CsvPracticeDataStore(_session.PracticeDataPath, _fileEncryption);
    }

    private static string ResolveInvoiceListPath(
        string invoiceListPath,
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath)
    {
        if (!string.IsNullOrWhiteSpace(invoiceListPath))
            return invoiceListPath;

        string? projectDirectory = GetDirectory(patientListPath)
            ?? GetDirectory(appointmentListPath)
            ?? GetDirectory(practiceDataPath);

        return string.IsNullOrWhiteSpace(projectDirectory)
            ? "invoices.csv"
            : Path.Combine(projectDirectory, "invoices.csv");
    }

    private static string? GetDirectory(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path);
    }

    private static bool TryCreateConversionEncryption(
        string outputDirectory,
        string password,
        out AesGcmFileEncryption encryption,
        out string error)
    {
        encryption = null!;
        error = "";

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            error = "Bitte gib einen Zielordner fuer die konvertierten Dateien an.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Bitte gib ein Passwort fuer die Konvertierung ein.";
            return false;
        }

        encryption = new AesGcmFileEncryption(password);
        return true;
    }

    private static Result ConvertProjectFiles(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath,
        string outputDirectory,
        Action<DatabaseFileConversion> convert,
        string operationName)
    {
        try
        {
            var files = GetDatabaseFiles(
                patientListPath,
                appointmentListPath,
                practiceDataPath,
                invoiceListPath,
                outputDirectory);

            foreach (var file in files)
            {
                convert(file);
            }

            return new Result(true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"{operationName} fehlgeschlagen: {ex.Message}");
        }
    }

    private static List<DatabaseFileConversion> GetDatabaseFiles(
        string patientListPath,
        string appointmentListPath,
        string practiceDataPath,
        string invoiceListPath,
        string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var files = new List<DatabaseFileConversion>
        {
            CreateRequiredConversion(patientListPath, outputDirectory, "Patientenliste"),
            CreateRequiredConversion(appointmentListPath, outputDirectory, "Terminliste"),
            CreateRequiredConversion(practiceDataPath, outputDirectory, "Praxisdaten")
        };

        if (!string.IsNullOrWhiteSpace(invoiceListPath) && File.Exists(invoiceListPath))
        {
            files.Add(CreateConversion(invoiceListPath, outputDirectory));
        }

        if (files.Count < RequiredDatabaseFileCount)
            throw new InvalidOperationException("Es wurden nicht alle Pflichtdateien fuer die Konvertierung gefunden.");

        return files;
    }

    private static DatabaseFileConversion CreateRequiredConversion(string sourcePath, string outputDirectory, string label)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new InvalidOperationException($"{label}: Bitte gib einen Dateipfad an.");

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"{label} wurde nicht gefunden.", sourcePath);

        return CreateConversion(sourcePath, outputDirectory);
    }

    private static DatabaseFileConversion CreateConversion(string sourcePath, string outputDirectory)
    {
        string targetPath = Path.Combine(outputDirectory, Path.GetFileName(sourcePath));
        if (Path.GetFullPath(sourcePath) == Path.GetFullPath(targetPath))
            throw new InvalidOperationException("Der Zielordner darf nicht identisch mit dem Quellordner sein.");

        return new DatabaseFileConversion(sourcePath, targetPath);
    }

    private sealed record DatabaseFileConversion(string SourcePath, string TargetPath);
}
