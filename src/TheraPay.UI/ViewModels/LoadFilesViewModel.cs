using System;
using TheraPay.Core;
using TheraPay.Infrastructure.csv;
using TheraPay.UI.Navigation;

namespace TheraPay.UI.ViewModels;

public sealed class LoadFilesViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly InMemoryPatientRepository _patientRepository;

    public RelayCommand LoadCommand { get; }
    public RelayCommand StartEmptyProjectCommand { get; }

    private string _patientListPath = "";
    public string PatientListPath
    {
        get => _patientListPath;
        set
        {
            if (_patientListPath == value) return;
            _patientListPath = value;
            OnPropertyChanged();
        }
    }

    private string _appointmentListPath = "";
    public string AppointmentListPath
    {
        get => _appointmentListPath;
        set
        {
            if (_appointmentListPath == value) return;
            _appointmentListPath = value;
            OnPropertyChanged();
        }
    }

    private string _practiceDataPath = "";
    public string PracticeDataPath
    {
        get => _practiceDataPath;
        set
        {
            if (_practiceDataPath == value) return;
            _practiceDataPath = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public LoadFilesViewModel(NavigationService nav, InMemoryPatientRepository patientRepository)
    {
        _nav = nav;
        _patientRepository = patientRepository;

        LoadCommand = new RelayCommand(LoadProject);
        StartEmptyProjectCommand = new RelayCommand(StartEmptyProject);
    }

    private void LoadProject()
    {
        if (string.IsNullOrWhiteSpace(PatientListPath))
        {
            StatusMessage = "Bitte gib mindestens einen Pfad zur Patientenliste an.";
            return;
        }

        try
        {
            IDataPersistence persistence = new CsvDataPersistence(new CsvPatientStore(PatientListPath));
            persistence.LoadInto(_patientRepository);

            // Terminliste und Praxisdaten sind bewusst noch nicht angebunden.
            _ = AppointmentListPath;
            _ = PracticeDataPath;

            _nav.NavigateTo<HomeViewModel>();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Laden fehlgeschlagen: {ex.Message}";
        }
    }

    private void StartEmptyProject()
    {
        // MVP: Start mit leerem In-Memory-Stand.
        _nav.NavigateTo<HomeViewModel>();
    }
}
