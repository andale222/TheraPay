using System;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.Infrastructure.csv;
using TheraPay.UI.State;

namespace TheraPay.UI.Services;

public sealed class ProjectPersistenceService
{
    private readonly IPatientRepository _patientRepository;
    private readonly ProjectSession _session;

    public ProjectPersistenceService(IPatientRepository patientRepository, ProjectSession session)
    {
        _patientRepository = patientRepository;
        _session = session;
    }

    public Result LoadProject(string patientListPath)
    {
        if (string.IsNullOrWhiteSpace(patientListPath))
        {
            return new Result(false, "Bitte gib mindestens einen Pfad zur Patientenliste an.");
        }

        try
        {
            _session.SetPatientListPath(patientListPath);
            _patientRepository.Clear();
            CreatePersistence().LoadInto(_patientRepository);
            _session.MarkSaved();
            return new Result(true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"Laden fehlgeschlagen: {ex.Message}");
        }
    }

    public void StartEmptyProject(string patientListPath)
    {
        _patientRepository.Clear();

        if (!string.IsNullOrWhiteSpace(patientListPath))
        {
            _session.SetPatientListPath(patientListPath);
        }

        _session.MarkSaved();
    }

    public Result SaveProject()
    {
        if (!_session.HasPatientListPath)
        {
            return new Result(false, "Kein Speicherpfad gesetzt. Bitte zuerst ein Projekt laden oder im Startscreen einen Projektpfad angeben.");
        }

        try
        {
            CreatePersistence().SaveFrom(_patientRepository);
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
        return new CsvDataPersistence(new CsvPatientStore(_session.PatientListPath));
    }
}
