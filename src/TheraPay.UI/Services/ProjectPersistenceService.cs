using System;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.Infrastructure.csv;
using TheraPay.UI.State;

namespace TheraPay.UI.Services;

public sealed class ProjectPersistenceService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ProjectSession _session;

    public ProjectPersistenceService(IPatientRepository patientRepository, IAppointmentRepository appointmentRepository, ProjectSession session)
    {
        _patientRepository = patientRepository;
        _appointmentRepository = appointmentRepository;
        _session = session;
    }

    public Result LoadProject(string patientListPath, string appointmentListPath, string practiceDataPath)
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
            _session.SetPatientListPath(patientListPath);
            _session.SetAppointmentListPath(appointmentListPath);
            _session.SetPracticeDataPath(practiceDataPath);
            _patientRepository.Clear();
            _appointmentRepository.Clear();
            CreatePersistence().LoadInto(_patientRepository, _appointmentRepository);
            _session.MarkSaved();
            return new Result(true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"Laden fehlgeschlagen: {ex.Message}");
        }
    }

    public void StartEmptyProject(string patientListPath, string appointmentListPath, string practiceDataPath)
    {
        _patientRepository.Clear();
        _appointmentRepository.Clear();

        if (!string.IsNullOrWhiteSpace(patientListPath))
        {
            _session.SetPatientListPath(patientListPath);
        }

        if (!string.IsNullOrWhiteSpace(appointmentListPath))
        {
            _session.SetAppointmentListPath(appointmentListPath);
        }

        if (!string.IsNullOrWhiteSpace(practiceDataPath))
        {
            _session.SetPracticeDataPath(practiceDataPath);
        }

        _session.MarkSaved();
    }

    public Result SaveProject()
    {
        if (!_session.HasPatientListPath || !_session.HasAppointmentListPath)
        {
            return new Result(false, "Kein Speicherpfad gesetzt. Bitte zuerst ein Projekt laden oder im Startscreen einen Projektpfad angeben.");
        }

        try
        {
            CreatePersistence().SaveFrom(_patientRepository, _appointmentRepository);
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
        return new CsvDataPersistence(new CsvPatientStore(_session.PatientListPath), 
        new CsvAppointmentStore(_session.AppointmentListPath));
    }
    private IPracticeDataStore CreatePracticeDataStore()
    {
        return new CsvPracticeDataStore(_session.PracticeDataPath);
    }
}
