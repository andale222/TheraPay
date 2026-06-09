using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TheraPay.Domain;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.Services;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class PatientPanelViewModel : ViewModelBase
{
    private const string DeletePatientWarning =
        "Achtung, der Patient wird gelöscht! Diese Aktion ist nicht widerrufbar, wollen Sie fortfahren?\n" +
        "Der Patient wird weiterhin gespeichert, aber nicht mehr angezeigt und nutzbar sein, das gilt auch für die ID.";
    private readonly PatientService _patients;
    private readonly NavigationService _nav;
    private readonly IMessageBoxService? _messageBox;
    private readonly ProjectSession? _session;
    private Func<Patient, bool>? _additionalPatientFilter;

    public ObservableCollection<PatientRowVm> Patients { get; } = new();

    private bool _showOnlyActivePatients;
    public bool ShowOnlyActivePatients
    {
        get => _showOnlyActivePatients;
        set
        {
            if (_showOnlyActivePatients == value) return;
            _showOnlyActivePatients = value;
            if (value)
            {
                _filterAll = false;
                _filterActive = true;
                _filterArchived = false;
                OnPropertyChanged(nameof(FilterAll));
                OnPropertyChanged(nameof(FilterActive));
                OnPropertyChanged(nameof(FilterArchived));
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowActivityFilter));
            Reload();
        }
    }

    public bool ShowActivityFilter => ShowOnlyActivePatients == false;

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
            _deleteCommand.RaiseCanExecuteChanged();
        }
    }

    // Filter (für MVP reicht es, wenn die Umschaltung schon funktioniert)
    private bool _filterAll;
    public bool FilterAll
    {
        get => _filterAll;
        set
        {
            if (ShowOnlyActivePatients && value) return;
            if (_filterAll == value) return;
            _filterAll = value;
            if (value) { _filterActive = false; _filterArchived = false; OnPropertyChanged(nameof(FilterActive)); OnPropertyChanged(nameof(FilterArchived)); }
            OnPropertyChanged();
            Reload();
        }
    }

    private bool _filterActive = true;
    public bool FilterActive
    {
        get => _filterActive;
        set
        {
            if (ShowOnlyActivePatients && value == false) return;
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
            if (ShowOnlyActivePatients && value) return;
            if (_filterArchived == value) return;
            _filterArchived = value;
            if (value) { _filterAll = false; _filterActive = false; OnPropertyChanged(nameof(FilterAll)); OnPropertyChanged(nameof(FilterActive)); }
            OnPropertyChanged();
            Reload();
        }
    }

    private readonly RelayCommand _editCommand;
    private readonly RelayCommand _deleteCommand;

    public ICommand EditCommand => _editCommand;
    public ICommand DeleteCommand => _deleteCommand;

    public PatientPanelViewModel(
        PatientService patientService,
        NavigationService nav,
        IMessageBoxService? messageBox = null,
        ProjectSession? session = null)
    {
        _patients = patientService;
        _nav = nav;
        _messageBox = messageBox;
        _session = session;

        _editCommand = new RelayCommand(
            execute: () => _nav.NavigateTo<PatientsViewModel>(vm => vm.LoadPatientForEdit(SelectedPatient!.Id)),
            canExecute: () => SelectedPatient is not null);

        _deleteCommand = new RelayCommand(
            execute: async () => await DeleteSelectedPatientAsync(),
            canExecute: () => SelectedPatient is not null);

        Reload();
    }

    public async Task DeleteSelectedPatientAsync()
    {
        if (SelectedPatient is null || _messageBox is null)
            return;

        var confirmed = await _messageBox.ConfirmWarningAsync(
            "Patient löschen",
            DeletePatientWarning,
            "Löschen",
            "Abbrechen");
        if (!confirmed)
            return;

        var result = _patients.SoftDeletePatient(SelectedPatient.Id);
        if (!result.Ok)
        {
            await _messageBox.ShowErrorAsync("Patient konnte nicht gelöscht werden", result.Error ?? "Patient konnte nicht gelöscht werden.");
            return;
        }

        _session?.MarkUnsavedChanges();
        Reload();
    }

    public void SetAdditionalPatientFilter(Func<Patient, bool>? filter)
    {
        _additionalPatientFilter = filter;
        Reload();
    }

    private void Reload()
    {
        string? selectedPatientId = SelectedPatient?.Id;
        Patients.Clear();

        var all = _patients.ViewPatients().AsEnumerable();

        if (ShowOnlyActivePatients || FilterActive)
            all = all.Where(p => p.IsActive);
        else if (FilterArchived)
            all = all.Where(p => p.IsActive == false);

        if (_additionalPatientFilter is not null)
            all = all.Where(_additionalPatientFilter);

        foreach (var p in all)
        {
            Patients.Add(new PatientRowVm
            {
                Id = p.ID,
                Anrede = p.Salutation,
                Name = p.LastName,
                Vorname = p.FirstName,
                Geburtsdatum = FormatDateOfBirth(p.DateOfBirth),
                Adresse = FormatAddress(p.Address),
                Email = p.Email,
                Telefon = p.PhoneNumber,
                Versicherungsart = p.InsuranceStatus.ToString(),
                Diagnose = p.ICD10Diagnosis
            });
        }

        SelectedPatient = Patients.FirstOrDefault(patient => patient.Id == selectedPatientId)
            ?? Patients.FirstOrDefault();
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

    private static string FormatDateOfBirth(DateOnly? dateOfBirth)
    {
        return dateOfBirth?.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE")) ?? "";
    }

    public sealed class PatientRowVm
    {
        public string Id { get; init; } = "";
        public string Anrede { get; init; } = "";
        public string Name { get; init; } = "";
        public string Vorname { get; init; } = "";
        public string Geburtsdatum { get; init; } = "";
        public string Adresse { get; init; } = "plchldr";
        public string Email { get; init; } = "plchldr";
        public string Telefon { get; init; } = "plchldr";
        public string Versicherungsart { get; init; } = "plchldr";
        public string Diagnose { get; init; } = "plchldr";
    }
}
