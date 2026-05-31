using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels.Panels;

namespace TheraPay.UI.ViewModels;

public class InvoiceCreationViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly IPatientRepository _store;
    private readonly ProjectSession _session;
    private readonly AppointmentService _appointmentService;
    private readonly BillingService _billingService;
    private DateTime? _issueDate = DateTime.Now.Date; // heute
    public DateTime? IssueDate
    {
        get => _issueDate;
        set { _issueDate = value; OnPropertyChanged(); }
    }
    private int _paymentTermInDays = 10;
    public int PaymentTermInDays
    {
        get => _paymentTermInDays;
        set { _paymentTermInDays = value; OnPropertyChanged(); }
    }
    private string _additionalText = "";
    public string AdditionalText
    {
        get => _additionalText;
        set { _additionalText = value; OnPropertyChanged(); }
    }
    public ObservableCollection<AppointmentRowVm> Appointments { get; } = new();
    public PatientPanelViewModel PatientsPanel { get; }
    public RelayCommand NavigateHomeViewCommand { get; }
    public RelayCommand NavigateInvoiceDraftCommand { get; }
    private string _draftStatusMessage = "";
    public string DraftStatusMessage
    {
        get => _draftStatusMessage;
        private set
        {
            if (_draftStatusMessage == value) return;
            _draftStatusMessage = value;
            OnPropertyChanged();
        }
    }


    public InvoiceCreationViewModel(PatientService patientService, AppointmentService appointmentService, BillingService billingService, PatientPanelViewModel patientsPanel, IPatientRepository store, ProjectSession session, NavigationService nav)
    {
        _nav = nav;
        _store = store;
        _session = session;
        _appointmentService = appointmentService;
        _billingService = billingService;
        PatientsPanel = patientsPanel;
        PatientsPanel.PropertyChanged += OnPatientsPanelPropertyChanged;

        _paymentTermInDays = _session.PracticeData.DefaultPaymentTermDays;
        NavigateHomeViewCommand = new RelayCommand(() => _nav.NavigateTo<HomeViewModel>());
        NavigateInvoiceDraftCommand = new RelayCommand(ContinueToDraft);

        ReloadAppointments();
    }

    public void ReloadAppointments()
    {
        Appointments.Clear();

        var selectedPatientId = PatientsPanel.SelectedPatient?.Id;
        if (string.IsNullOrWhiteSpace(selectedPatientId))
        {
            return;
        }

        var appointments = _appointmentService
            .GetNotBilledAppointmentsForPatient(selectedPatientId)
            .OrderBy(appt => appt.Date);

        foreach (var appt in appointments)
        {
            Appointments.Add(new AppointmentRowVm
            {
                Id = appt.Id.ToString(),
                Date = appt.Date.ToString("dd.MM.yy HH:mm"),
                Duration = $"{appt.DurationInMinutes} min",
                PatientName = $"{appt.PatientID}",
                AppointmentName = "TODO:",
                IsSelected = true,
                BillingState = $"{appt.Status}"
            });
        }
    }

    private void ContinueToDraft()
    {
        var selectedPatientId = PatientsPanel.SelectedPatient?.Id;
        if (string.IsNullOrWhiteSpace(selectedPatientId))
        {
            DraftStatusMessage = "Bitte zuerst einen Patienten auswählen.";
            return;
        }

        var selectedAppointmentIds = GetCheckedAppointmentIds();
        if (selectedAppointmentIds.Count == 0)
        {
            DraftStatusMessage = "Bitte mindestens einen Termin zum Abrechnen auswählen.";
            return;
        }

        var result = CreateInvoiceDraft(selectedPatientId, selectedAppointmentIds);
        DraftStatusMessage = result.Ok
            ? string.IsNullOrWhiteSpace(result.Error) ? "Invoice-Draft wurde erstellt." : result.Error
            : (result.Error ?? "Invoice-Draft konnte nicht erstellt werden.");

        if (result.Ok)
        {
            _session.MarkUnsavedChanges();
            _nav.NavigateTo<InvoiceDraftViewModel>();
        }
    }

    private List<Guid> GetCheckedAppointmentIds()
    {
        var selectedIds = new List<Guid>();

        foreach (var row in Appointments.Where(a => a.IsSelected))
        {
            if (Guid.TryParse(row.Id, out var appointmentId))
            {
                selectedIds.Add(appointmentId);
            }
        }

        return selectedIds;
    }

    private Result CreateInvoiceDraft(string patientId, List<Guid> appointmentIds)
    {
        return _billingService.AddInvoiceForPatientAndAppointments(
            patientId,
            appointmentIds,
            _session.PracticeData);
    }

    private void OnPatientsPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) ||
            e.PropertyName == nameof(PatientPanelViewModel.SelectedPatient))
        {
            ReloadAppointments();
        }
    }

    public sealed class AppointmentRowVm
    {
        public string Id { get; init; } = "";
        public string Date { get; init; } = "";
        public string Duration { get; init; } = "";
        public string PatientName { get; init; } = "plchldr";
        public string AppointmentName { get; init; } = "plchldr";
        public bool IsSelected { get; set; } = true;
        public string BillingState { get; init; } = "";
    }
}
