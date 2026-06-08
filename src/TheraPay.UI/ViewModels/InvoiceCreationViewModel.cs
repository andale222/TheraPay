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
    private bool _draftConflictConfirmationRequired;
    private string _draftConflictSelectionSignature = "";
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
    private string _subject = Invoice.DefaultSubject;
    public string Subject
    {
        get => _subject;
        set { _subject = value; OnPropertyChanged(); }
    }
    public ObservableCollection<AppointmentRowVm> Appointments { get; } = new();
    public PatientPanelViewModel PatientsPanel { get; }
    public RelayCommand NavigateHomeViewCommand { get; }
    public RelayCommand NavigateInvoiceDraftCommand { get; }
    public string DraftActionText => _draftConflictConfirmationRequired ? "Trotzdem Draft erstellen" : "Weiter zum Draft";
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
        ResetDraftConflictConfirmation();
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
            ResetDraftConflictConfirmation();
            DraftStatusMessage = "Bitte zuerst einen Patienten auswählen.";
            return;
        }

        var selectedAppointmentIds = GetCheckedAppointmentIds();
        if (selectedAppointmentIds.Count == 0)
        {
            ResetDraftConflictConfirmation();
            DraftStatusMessage = "Bitte mindestens einen Termin zum Abrechnen auswählen.";
            return;
        }

        var selectedAppointmentSignature = BuildAppointmentSelectionSignature(selectedAppointmentIds);
        var draftConflicts = FindDraftAppointmentConflicts(selectedAppointmentIds);
        if (draftConflicts.Count > 0 &&
            (!_draftConflictConfirmationRequired || _draftConflictSelectionSignature != selectedAppointmentSignature))
        {
            _draftConflictConfirmationRequired = true;
            _draftConflictSelectionSignature = selectedAppointmentSignature;
            OnPropertyChanged(nameof(DraftActionText));
            DraftStatusMessage = BuildDraftConflictWarning(draftConflicts);
            return;
        }

        var result = CreateInvoiceDraft(selectedPatientId, selectedAppointmentIds);
        ResetDraftConflictConfirmation();
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

    private void ResetDraftConflictConfirmation()
    {
        if (!_draftConflictConfirmationRequired && string.IsNullOrWhiteSpace(_draftConflictSelectionSignature))
            return;

        _draftConflictConfirmationRequired = false;
        _draftConflictSelectionSignature = "";
        OnPropertyChanged(nameof(DraftActionText));
    }

    private static string BuildAppointmentSelectionSignature(IEnumerable<Guid> appointmentIds)
    {
        return string.Join("|", appointmentIds.OrderBy(id => id).Select(id => id.ToString("D")));
    }

    private List<DraftAppointmentConflict> FindDraftAppointmentConflicts(List<Guid> appointmentIds)
    {
        var selectedAppointmentIds = appointmentIds
            .Select(id => id.ToString("D"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _billingService
            .ViewInvoices()
            .Where(invoice => invoice.Status == InvoiceStatus.Draft)
            .SelectMany(invoice => invoice.AppointmentDataList
                .Where(appointment => selectedAppointmentIds.Contains(appointment.AppointmentId))
                .Select(appointment => new DraftAppointmentConflict(
                    invoice.Id,
                    appointment.AppointmentId,
                    appointment.Date)))
            .ToList();
    }

    private static string BuildDraftConflictWarning(IReadOnlyList<DraftAppointmentConflict> conflicts)
    {
        var details = string.Join("; ", conflicts
            .Take(3)
            .Select(conflict => $"{conflict.AppointmentDate:dd.MM.yy HH:mm} in Draft {conflict.DraftId:D}"));
        var additionalConflictsText = conflicts.Count > 3
            ? $" (+{conflicts.Count - 3} weitere)"
            : "";

        return $"Achtung: Ausgewählte Termine sind bereits in anderen Drafts enthalten: {details}{additionalConflictsText}. Klicke erneut auf \"Trotzdem Draft erstellen\", wenn du trotzdem fortfahren möchtest.";
    }

    private Result CreateInvoiceDraft(string patientId, List<Guid> appointmentIds)
    {
        return _billingService.AddInvoiceForPatientAndAppointments(
            patientId,
            appointmentIds,
            _session.PracticeData,
            IssueDate ?? DateTime.Today,
            PaymentTermInDays,
            AdditionalText,
            Subject);
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

    private sealed record DraftAppointmentConflict(Guid DraftId, string AppointmentId, DateTime AppointmentDate);
}
