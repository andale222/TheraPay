using System.Windows.Input;
using TheraPay.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using TheraPay.UI.ViewModels.Panels;

namespace TheraPay.UI.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly NavigationStore _store;
    public PatientPanelViewModel PatientsPanel { get; }
    public CalendarPanelViewModel CalendarPanel { get; }

    public object? CurrentViewModel => _store.CurrentViewModel;
    public ICommand NavigatePatientsCommand  { get; }
    public ICommand NavigateEditAppointmentCommand  { get; }

    public HomeViewModel(PatientPanelViewModel patientsPanel, CalendarPanelViewModel calendarPanel, NavigationStore store, NavigationService nav)
    {
        PatientsPanel = patientsPanel;
        CalendarPanel = calendarPanel;
        _store = store;
        _store.CurrentViewModelChanged += (_, __) => OnPropertyChanged(nameof(CurrentViewModel));

        NavigatePatientsCommand  = new RelayCommand(() => nav.NavigateTo<PatientsViewModel>());
        NavigateEditAppointmentCommand  = new RelayCommand(() => nav.NavigateTo<AppointmentEditViewModel>());
    }
}