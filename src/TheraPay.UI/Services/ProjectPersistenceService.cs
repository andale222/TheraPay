using System;
using System.IO;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.Infrastructure.csv;
using TheraPay.UI.State;

namespace TheraPay.UI.Services;

public sealed class ProjectPersistenceService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ProjectSession _session;
    private ICsvFileEncryption _fileEncryption = MockCsvFileEncryption.Instance;

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
        ICsvFileEncryption? fileEncryption = null)
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
            _fileEncryption = fileEncryption ?? MockCsvFileEncryption.Instance;
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
        ICsvFileEncryption? fileEncryption = null)
    {
        _fileEncryption = fileEncryption ?? MockCsvFileEncryption.Instance;
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
}
