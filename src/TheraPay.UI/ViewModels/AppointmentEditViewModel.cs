using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.ViewModels.Panels;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public sealed class AppointmentEditViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly ProjectSession _session;
    private readonly AppointmentService _appointmentService;
    private Guid? _editingAppointmentId;
    public CalendarPanelViewModel CalendarPanel { get; }
    public PatientPanelViewModel PatientsPanel { get; }

    public RelayCommand NavigateHomeViewCommand { get; }
    public RelayCommand SaveAppointmentCommand { get; }
    public RelayCommand SaveAndAddAnotherAppointmentCommand { get; }
    public RelayCommand CheckDataCommand { get; }
    public RelayCommand AddBillingNumberCommand { get; }
    public RelayCommand RemoveBillingNumberCommand { get; }

    public ObservableCollection<BillingNumber> AvailableBillingNumbers { get; } = new(BillingNumberCatalog.GetDefaultNumbers());
    public ObservableCollection<BillingNumber> AssignedBillingNumbers { get; } = [];
    public bool IsEditing => _editingAppointmentId.HasValue;
    public string ViewTitle => IsEditing ? "Termin bearbeiten" : "Termin hinzufügen";
    public string SaveButtonText => IsEditing ? "Änderungen speichern + Zurück" : "Speichern + Zurück";

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    private BillingNumber? _selectedBillingNumber;
    public BillingNumber? SelectedBillingNumber
    {
        get => _selectedBillingNumber;
        set
        {
            _selectedBillingNumber = value;
            LoadBillingNumberDraft(value);
            OnPropertyChanged();
        }
    }

    private BillingNumber? _selectedAssignedBillingNumber;
    public BillingNumber? SelectedAssignedBillingNumber
    {
        get => _selectedAssignedBillingNumber;
        set
        {
            _selectedAssignedBillingNumber = value;
            LoadAssignedBillingNumberDraft(value);
            OnPropertyChanged();
        }
    }

    public decimal TotalAmountPreview => AssignedBillingNumbers.Sum(billingNumber => billingNumber.Amount);
    public decimal DraftAmountPreview => TryReadDraftAmount(out var amount) ? amount : 0m;

    private string _draftDescription = "";
    public string DraftDescription
    {
        get => _draftDescription;
        set { _draftDescription = value; OnPropertyChanged(); }
    }

    private string _draftFactor = "";
    public string DraftFactor
    {
        get => _draftFactor;
        set
        {
            _draftFactor = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DraftAmountPreview));
        }
    }

    private string _draftBaseValue = "";
    public string DraftBaseValue
    {
        get => _draftBaseValue;
        set
        {
            _draftBaseValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DraftAmountPreview));
        }
    }

    private DateTime? _startDate = DateTime.Now.Date; // heute
    public DateTime? StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(); NotifyCalculated(); }
    }
    private TimeSpan? _startTime = DateTime.Now.TimeOfDay;
    public TimeSpan? StartTime
    {
        get => _startTime;
        set { _startTime = value; OnPropertyChanged(); NotifyCalculated(); }
    }
    private TimeSpan? _endTime = DateTime.Now.TimeOfDay;
    public TimeSpan? EndTime
    {
        get => _endTime;
        set { _endTime = value; OnPropertyChanged(); NotifyCalculated(); }
    }

    public int? DurationInMinutes
    {
        get
        {
            if (StartTime is null || EndTime is null) return null;

            var diff = EndTime.Value - StartTime.Value;
            return diff.TotalMinutes >= 0 ? (int)diff.TotalMinutes : null; // oder über Mitternacht behandeln
        }
    }


    public AppointmentEditViewModel(AppointmentService appointmentService, CalendarPanelViewModel calendarPanel, PatientPanelViewModel patientsPanel, NavigationService nav, ProjectSession session)
    {
        _nav = nav;
        _session = session;
        _appointmentService = appointmentService;
        CalendarPanel = calendarPanel;
        PatientsPanel = patientsPanel;
        PatientsPanel.ShowOnlyActivePatients = true;

        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        SaveAppointmentCommand = new RelayCommand(SaveAppointment);
        SaveAndAddAnotherAppointmentCommand = new RelayCommand(SaveAndAddAnotherAppointment);
        CheckDataCommand = new RelayCommand(CheckData);
        AddBillingNumberCommand = new RelayCommand(AddSelectedBillingNumber);
        RemoveBillingNumberCommand = new RelayCommand(RemoveSelectedBillingNumber);
        SelectedBillingNumber = AvailableBillingNumbers.FirstOrDefault();
    }

    public void SelectPatient(string patientId)
    {
        PatientsPanel.SelectPatient(patientId);
    }

    private void NotifyCalculated()
        => OnPropertyChanged(nameof(DurationInMinutes));


    private void SaveAppointment()
    {
        if (TrySaveAppointment(out _))
        {
            NavigateHomeViewCommand.Execute(null);
        }
    }

    private void SaveAndAddAnotherAppointment()
    {
        if (TrySaveAppointment(out var createdAppointment))
        {
            ResetFormForNextAppointment(createdAppointment);
        }
    }

    private bool TrySaveAppointment(out Appointment? createdAppointment)
    {
        createdAppointment = null;

        if (StartDate is null || StartTime is null || EndTime is null || DurationInMinutes is null)
        {
            StatusMessage = "Bitte Datum, Start- und Endzeit angeben.";
            return false;
        }

        var selectedPatientId = PatientsPanel.SelectedPatient?.Id;
        if (string.IsNullOrWhiteSpace(selectedPatientId))
        {
            StatusMessage = "Bitte einen Patienten auswählen.";
            return false;
        }

        DateTime startDateTime = StartDate.Value.Date + StartTime.Value;
        var billingNumbers = AssignedBillingNumbers.ToList();
        var result = _editingAppointmentId is Guid editingAppointmentId
            ? _appointmentService.UpdateAppointment(
                editingAppointmentId,
                startDateTime,
                selectedPatientId,
                DurationInMinutes ?? 0,
                billingNumbers)
            : _appointmentService.AddAppointment(
                startDateTime,
                selectedPatientId,
                DurationInMinutes ?? 0,
                billingNumbers);

        if (!result.Ok)
        {
            StatusMessage = result.Error ?? "Termin konnte nicht gespeichert werden.";
            return false;
        }

        _session.MarkUnsavedChanges();
        createdAppointment = null;
        return true;
    }

    private void ResetFormForNextAppointment(Appointment? createdAppointment)
    {
        _editingAppointmentId = null;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(ViewTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        CalendarPanel.ReloadAppointments();
        StatusMessage = "Termin gespeichert. Sie können direkt einen weiteren anlegen.";
    }

    public void LoadAppointmentForEdit(Guid appointmentId)
    {
        try
        {
            var appointment = _appointmentService.GetAppointmentById(appointmentId);
            _editingAppointmentId = appointmentId;
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(ViewTitle));
            OnPropertyChanged(nameof(SaveButtonText));

            StartDate = appointment.Date.Date;
            StartTime = appointment.Date.TimeOfDay;
            EndTime = appointment.End.TimeOfDay;
            PatientsPanel.SelectPatient(appointment.PatientID);

            AssignedBillingNumbers.Clear();
            foreach (var billingNumber in appointment.BillingNumbers)
            {
                AssignedBillingNumbers.Add(billingNumber);
            }

            SelectedAssignedBillingNumber = AssignedBillingNumbers.FirstOrDefault();
            OnPropertyChanged(nameof(TotalAmountPreview));
            StatusMessage = "";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void CheckData()
    {
        // Plausibilitätsprüfungen, z.B.:
        // - Alle Felder ausgefüllt?
        // - Endzeit nach Startzeit?
        // - Überschneidungen mit anderen Terminen?
        // - Patient ausgewählt?
    }

    private void AddSelectedBillingNumber()
    {
        if (SelectedBillingNumber is null)
        {
            return;
        }

        if (!TryReadDraftValues(out var factor, out var baseValue))
        {
            return;
        }

        var billingNumber = new BillingNumber(
            SelectedBillingNumber.NumberIdentifier,
            factor,
            baseValue,
            DraftDescription,
            SelectedBillingNumber.Type);

        AssignedBillingNumbers.Add(billingNumber);
        SelectedAssignedBillingNumber = billingNumber;
        OnPropertyChanged(nameof(TotalAmountPreview));
    }

    private void RemoveSelectedBillingNumber()
    {
        if (SelectedAssignedBillingNumber is null)
        {
            return;
        }

        AssignedBillingNumbers.Remove(SelectedAssignedBillingNumber);
        SelectedAssignedBillingNumber = AssignedBillingNumbers.LastOrDefault();
        OnPropertyChanged(nameof(TotalAmountPreview));
    }

    private void LoadBillingNumberDraft(BillingNumber? billingNumber)
    {
        if (billingNumber is null)
        {
            DraftDescription = "";
            DraftFactor = "";
            DraftBaseValue = "";
            return;
        }

        DraftDescription = billingNumber.Description;
        DraftFactor = billingNumber.Factor.ToString("0.##", CultureInfo.CurrentCulture);
        DraftBaseValue = billingNumber.BaseValue.ToString("0.00", CultureInfo.CurrentCulture);
    }

    private void LoadAssignedBillingNumberDraft(BillingNumber? billingNumber)
    {
        if (billingNumber is null)
        {
            return;
        }

        var catalogMatch = AvailableBillingNumbers.FirstOrDefault(candidate =>
            string.Equals(candidate.NumberIdentifier, billingNumber.NumberIdentifier, StringComparison.OrdinalIgnoreCase)
            && candidate.Type == billingNumber.Type);

        _selectedBillingNumber = catalogMatch ?? billingNumber;
        OnPropertyChanged(nameof(SelectedBillingNumber));
        LoadBillingNumberDraft(billingNumber);
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        value ??= "";
        var styles = NumberStyles.Number;
        if (decimal.TryParse(value, styles, CultureInfo.CurrentCulture, out result))
        {
            return true;
        }

        if (decimal.TryParse(value, styles, CultureInfo.GetCultureInfo("de-DE"), out result))
        {
            return true;
        }

        if (decimal.TryParse(value, styles, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(value.Replace(',', '.'), styles, CultureInfo.InvariantCulture, out result);
    }

    private bool TryReadDraftValues(out decimal factor, out decimal baseValue)
    {
        var factorIsValid = TryParseDecimal(DraftFactor, out factor);
        var baseValueIsValid = TryParseDecimal(DraftBaseValue, out baseValue);
        return factorIsValid && baseValueIsValid;
    }

    private bool TryReadDraftAmount(out decimal amount)
    {
        if (!TryReadDraftValues(out var factor, out var baseValue))
        {
            amount = 0m;
            return false;
        }

        amount = Math.Round(factor * baseValue, 2, MidpointRounding.AwayFromZero);
        return true;
    }
}
