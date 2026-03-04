using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TheraPay.Domain;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class CalendarPanelViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly AppointmentService _appointmentService;
    
    public ObservableCollection<AppointmentRowVm> Appointments { get; } = new();

public CalendarPanelViewModel(NavigationService nav, AppointmentService appointmentService)
    {
        _nav = nav;
        _appointmentService = appointmentService;
        
        ReloadAppointments();
    }

    public void ReloadAppointments()
    {
        Appointments.Clear();
        foreach (var appt in _appointmentService.ViewAppointments())
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