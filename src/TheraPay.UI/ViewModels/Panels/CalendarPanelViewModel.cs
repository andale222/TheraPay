using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI;
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.ViewModels;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class CalendarPanelViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly AppointmentService _appointmentService;
    private readonly ProjectSession _session;
    private string? _pendingDeleteAppointmentId;
    
    public ObservableCollection<AppointmentRowVm> Appointments { get; } = new();
    public ICommand EditAppointmentCommand { get; }
    public ICommand DeleteAppointmentCommand { get; }

    private AppointmentRowVm? _selectedAppointment;
    public AppointmentRowVm? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (_selectedAppointment == value) return;
            _selectedAppointment = value;
            OnPropertyChanged();
            ResetDeleteConfirmation();
            EditAppointmentRelayCommand.RaiseCanExecuteChanged();
            DeleteAppointmentRelayCommand.RaiseCanExecuteChanged();
        }
    }

    private string _deleteButtonText = "Löschen";
    public string DeleteButtonText
    {
        get => _deleteButtonText;
        private set
        {
            if (_deleteButtonText == value) return;
            _deleteButtonText = value;
            OnPropertyChanged();
        }
    }

    private RelayCommand EditAppointmentRelayCommand => (RelayCommand)EditAppointmentCommand;
    private RelayCommand DeleteAppointmentRelayCommand => (RelayCommand)DeleteAppointmentCommand;

    private bool _filterAll = true;
    public bool FilterAll
    {
        get => _filterAll;
        set
        {
            if (_filterAll == value) return;
            _filterAll = value;
            OnPropertyChanged();

            if (value)
            {
                _filterSelectedDay = false;
                OnPropertyChanged(nameof(FilterSelectedDay));
            }

            ReloadAppointments();
        }
    }

    private bool _filterSelectedDay;
    public bool FilterSelectedDay
    {
        get => _filterSelectedDay;
        set
        {
            if (_filterSelectedDay == value) return;
            _filterSelectedDay = value;
            OnPropertyChanged();

            if (value)
            {
                _filterAll = false;
                OnPropertyChanged(nameof(FilterAll));
            }

            ReloadAppointments();
        }
    }

    private DateTime? _selectedDate = DateTime.Today;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate == value) return;
            _selectedDate = value;
            OnPropertyChanged();

            if (FilterSelectedDay)
            {
                ReloadAppointments();
            }
        }
    }

    public CalendarPanelViewModel(NavigationService nav, AppointmentService appointmentService, ProjectSession session)
    {
        _nav = nav;
        _appointmentService = appointmentService;
        _session = session;
        EditAppointmentCommand = new RelayCommand(EditSelectedAppointment, () => SelectedAppointment is not null);
        DeleteAppointmentCommand = new RelayCommand(DeleteSelectedAppointment, () => SelectedAppointment is not null);

        ReloadAppointments();
    }

    public void ReloadAppointments()
    {
        ResetDeleteConfirmation();
        Appointments.Clear();

        var appointments = FilterSelectedDay
            ? _appointmentService.GetAppointmentsByDate(SelectedDate ?? DateTime.Today)
            : _appointmentService.ViewAppointments();

        foreach (var appt in appointments)
        {
            Appointments.Add(new AppointmentRowVm
            {
                Id = appt.Id.ToString(),
                Date = appt.Date.ToString("dd.MM.yy HH:mm"),
                Duration = $"{appt.DurationInMinutes} min",
                PatientName = $"{appt.PatientID}",
                AppointmentName = appt.BillingNumbers.FirstOrDefault()?.Description ?? ""
            });
        }

        if (SelectedAppointment is not null && Appointments.All(appointment => appointment.Id != SelectedAppointment.Id))
        {
            SelectedAppointment = null;
        }
    }

    private void EditSelectedAppointment()
    {
        if (SelectedAppointment is null || !Guid.TryParse(SelectedAppointment.Id, out var appointmentId))
        {
            return;
        }

        _nav.NavigateTo<AppointmentEditViewModel>(viewModel => viewModel.LoadAppointmentForEdit(appointmentId));
    }

    private void DeleteSelectedAppointment()
    {
        if (SelectedAppointment is null || !Guid.TryParse(SelectedAppointment.Id, out var appointmentId))
        {
            return;
        }

        if (_pendingDeleteAppointmentId != SelectedAppointment.Id)
        {
            _pendingDeleteAppointmentId = SelectedAppointment.Id;
            DeleteButtonText = "Löschen bestätigen";
            return;
        }

        var result = _appointmentService.DeleteAppointment(appointmentId);
        if (!result.Ok)
        {
            ResetDeleteConfirmation();
            return;
        }

        _session.MarkUnsavedChanges();
        ResetDeleteConfirmation();
        ReloadAppointments();
    }

    private void ResetDeleteConfirmation()
    {
        _pendingDeleteAppointmentId = null;
        DeleteButtonText = "Löschen";
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
