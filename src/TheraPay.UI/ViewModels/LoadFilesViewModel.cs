using TheraPay.UI.Navigation;
using TheraPay.UI.Services;

namespace TheraPay.UI.ViewModels;

public sealed class LoadFilesViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly ProjectPersistenceService _projectPersistence;

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

    public LoadFilesViewModel(NavigationService nav, ProjectPersistenceService projectPersistence)
    {
        _nav = nav;
        _projectPersistence = projectPersistence;

        LoadCommand = new RelayCommand(LoadProject);
        StartEmptyProjectCommand = new RelayCommand(StartEmptyProject);
    }

    private void LoadProject()
    {
        var result = _projectPersistence.LoadProject(PatientListPath, AppointmentListPath, PracticeDataPath);
        if (!result.Ok)
        {
            StatusMessage = result.Error ?? "Laden fehlgeschlagen.";
            return;
        }

        StatusMessage = "";
        _nav.NavigateTo<HomeViewModel>();
    }

    private void StartEmptyProject()
    {
        _projectPersistence.StartEmptyProject(PatientListPath, AppointmentListPath, PracticeDataPath);
        StatusMessage = "";
        _nav.NavigateTo<HomeViewModel>();
    }
}
