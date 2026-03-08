using System;
using System.Collections.ObjectModel;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class CalendarPanelViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly AppointmentService _appointmentService;
    
    public ObservableCollection<AppointmentRowVm> Appointments { get; } = new();

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

    public CalendarPanelViewModel(NavigationService nav, AppointmentService appointmentService)
    {
        _nav = nav;
        _appointmentService = appointmentService;
        
        ReloadAppointments();
    }

    public void ReloadAppointments()
    {
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
                PatientName = "TODO:",
                AppointmentName = "TODO:"
            });
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
