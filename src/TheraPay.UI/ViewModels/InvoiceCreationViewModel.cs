using System;
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
    private readonly IPatientRepository _store;
    private readonly ProjectSession _session;
    private readonly AppointmentService _appointmentService;
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


    public InvoiceCreationViewModel(PatientService patientService, AppointmentService appointmentService, PatientPanelViewModel patientsPanel, IPatientRepository store, ProjectSession session, NavigationService nav)
    {
        _store = store;
        _session = session;
        _appointmentService = appointmentService;
        PatientsPanel = patientsPanel;
        PatientsPanel.PropertyChanged += OnPatientsPanelPropertyChanged;

        _paymentTermInDays = _session.PracticeData.DefaultPaymentTermDays;
        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        NavigateInvoiceDraftCommand = null;

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
                AppointmentName = "TODO:"
            });
        }
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
    }
}
