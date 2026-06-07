using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TheraPay.Domain;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class PatientPanelViewModel : ViewModelBase
{
    private readonly PatientService _patients;
    private readonly NavigationService _nav;

    public ObservableCollection<PatientRowVm> Patients { get; } = new();

    private PatientRowVm? _selectedPatient;
    public PatientRowVm? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (_selectedPatient == value) return;
            _selectedPatient = value;
            OnPropertyChanged();

            _editCommand.RaiseCanExecuteChanged();
            _toggleActiveCommand.RaiseCanExecuteChanged();
            _deleteCommand.RaiseCanExecuteChanged();
        }
    }

    // Filter (für MVP reicht es, wenn die Umschaltung schon funktioniert)
    private bool _filterAll = true;
    public bool FilterAll
    {
        get => _filterAll;
        set
        {
            if (_filterAll == value) return;
            _filterAll = value;
            if (value) { _filterActive = false; _filterArchived = false; OnPropertyChanged(nameof(FilterActive)); OnPropertyChanged(nameof(FilterArchived)); }
            OnPropertyChanged();
            Reload();
        }
    }

    private bool _filterActive;
    public bool FilterActive
    {
        get => _filterActive;
        set
        {
            if (_filterActive == value) return;
            _filterActive = value;
            if (value) { _filterAll = false; _filterArchived = false; OnPropertyChanged(nameof(FilterAll)); OnPropertyChanged(nameof(FilterArchived)); }
            OnPropertyChanged();
            Reload();
        }
    }

    private bool _filterArchived;
    public bool FilterArchived
    {
        get => _filterArchived;
        set
        {
            if (_filterArchived == value) return;
            _filterArchived = value;
            if (value) { _filterAll = false; _filterActive = false; OnPropertyChanged(nameof(FilterAll)); OnPropertyChanged(nameof(FilterActive)); }
            OnPropertyChanged();
            Reload();
        }
    }

    private readonly RelayCommand _editCommand;
    private readonly RelayCommand _toggleActiveCommand;
    private readonly RelayCommand _deleteCommand;

    public ICommand EditCommand => _editCommand;
    public ICommand ToggleActiveCommand => _toggleActiveCommand;
    public ICommand DeleteCommand => _deleteCommand;

    public PatientPanelViewModel(PatientService patientService, NavigationService nav)
    {
        _patients = patientService;
        _nav = nav;

        _editCommand = new RelayCommand(
            execute: () => _nav.NavigateTo<PatientsViewModel>(vm => vm.LoadPatientForEdit(SelectedPatient!.Id)),
            canExecute: () => SelectedPatient is not null);

        _toggleActiveCommand = new RelayCommand(
            execute: () =>
            {
                // TODO: sobald du im Core eine Toggle/Archive-Funktion hast:
                // _patients.ToggleActive(SelectedPatient!.Id);
                // Reload();

                _nav.NavigateTo<PatientsViewModel>(); // MVP: erst mal zur Patientenverwaltung springen
            },
            canExecute: () => false);

        _deleteCommand = new RelayCommand(
            execute: () =>
            {
                // TODO: SoftDelete im Core:
                // _patients.SoftDelete(SelectedPatient!.Id);
                // Reload();

                _nav.NavigateTo<PatientsViewModel>(); // MVP
            },
            canExecute: () => false);

        Reload();
    }

    private void Reload()
    {
        Patients.Clear();

        var all = _patients.ViewPatients();

        // MVP: Filterlogik optional. Wenn du später IsArchived/IsActive hast, hier filtern.
        // if (FilterActive) all = all.Where(p => p.IsActive).ToList();
        // if (FilterArchived) all = all.Where(p => p.IsArchived).ToList();

        foreach (var p in all)
        {
            Patients.Add(new PatientRowVm
            {
                Id = p.ID,
                Name = p.LastName,
                Vorname = p.FirstName,
                Adresse = FormatAddress(p.Address),
                Email = p.Email,
                Telefon = p.PhoneNumber,
                Versicherungsart = p.InsuranceStatus.ToString(),
                Diagnose = p.ICD10Diagnosis
            });
        }

        SelectedPatient = Patients.FirstOrDefault();
    }

    public void SelectPatient(string patientId)
    {
        SelectedPatient = Patients.FirstOrDefault(patient => patient.Id == patientId);
    }

    private static string FormatAddress(Address? address)
    {
        if (address is null)
            return "";

        return $"{address.GetStreetNr()}, {address.GetPostalCodeCity()}";
    }

    public sealed class PatientRowVm
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string Vorname { get; init; } = "";
        public string Geburtsdatum { get; init; } = "plchldr";
        public string Adresse { get; init; } = "plchldr";
        public string Email { get; init; } = "plchldr";
        public string Telefon { get; init; } = "plchldr";
        public string Versicherungsart { get; init; } = "plchldr";
        public string Diagnose { get; init; } = "plchldr";
    }
}
