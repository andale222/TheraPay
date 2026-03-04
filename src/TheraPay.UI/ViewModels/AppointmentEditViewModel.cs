using System;
using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.ViewModels.Panels;

namespace TheraPay.UI.ViewModels;

public sealed class AppointmentEditViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    public CalendarPanelViewModel CalendarPanel { get; }
    public PatientPanelViewModel PatientsPanel { get; }

    public RelayCommand NavigateHomeViewCommand  { get; }
    public RelayCommand SaveAppointmentCommand  { get; }
    public RelayCommand CheckDataCommand  { get; }

    private DateTime? _startDate = DateTime.Now.Date; // heute
    public DateTime? StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(); }
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


public AppointmentEditViewModel(AppointmentService appointmentService, CalendarPanelViewModel calendarPanel, PatientPanelViewModel patientsPanel, NavigationService nav)
    {
        _nav = nav;
        CalendarPanel = calendarPanel;
        PatientsPanel = patientsPanel;

        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        SaveAppointmentCommand = new RelayCommand(SaveAppointment);
        CheckDataCommand = new RelayCommand(CheckData);
    }

    private void NotifyCalculated()
        => OnPropertyChanged(nameof(DurationInMinutes));


    private void SaveAppointment()
    {
        
    }
    private void CheckData()
    {
        // Plausibilitätsprüfungen, z.B.:
        // - Alle Felder ausgefüllt?
        // - Endzeit nach Startzeit?
        // - Überschneidungen mit anderen Terminen?
        // - Patient ausgewählt?
    }
}

