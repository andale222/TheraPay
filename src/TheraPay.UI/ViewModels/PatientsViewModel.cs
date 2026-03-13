using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public class PatientsViewModel : ViewModelBase
{
    private readonly IPatientRepository _store;
    private readonly ProjectSession _session;
    public RelayCommand NavigateHomeViewCommand  { get; }
    public RelayCommand AddPatientCommand  { get; }
    public RelayCommand CheckDataCommand  { get; }

    public ObservableCollection<Patient> Patients { get; } = new();

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); }
    }

    private string _lastName = "";
    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); }
    }
    private string _patientID = "";
    public string PatientID
{
    get => _patientID;
    set
    {
        if (_patientID != value)
        {
            _patientID = value;
            OnPropertyChanged();
        }
    }
}

    public PatientsViewModel(PatientService patientService, IPatientRepository store, ProjectSession session, NavigationService nav)
    {
        _store = store;
        _session = session;
        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        AddPatientCommand = new RelayCommand(AddPatient);
        CheckDataCommand = new RelayCommand(CheckData);
        Reload();
    }

    private void AddPatient()
    {
        var p = new Patient(FirstName.Trim(), LastName.Trim(),PatientID);
        var addResult = _store.Add(p);
        if (addResult.Ok)
        {
            _session.MarkUnsavedChanges();
        }

        FirstName = "";
        LastName = "";
        PatientID = "";
        Reload();

        NavigateHomeViewCommand.Execute(null);
    }

    private void CheckData()
    {
        // TODO: Implement data checking logic here
    }

    private void Reload()
    {
        Patients.Clear();
        foreach (var p in _store.GetAll().OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            Patients.Add(p);
    }
}