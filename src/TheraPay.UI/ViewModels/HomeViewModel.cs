using System.Windows.Input;
using TheraPay.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using TheraPay.UI.ViewModels.Panels;

namespace TheraPay.UI.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly NavigationStore _store;
    public PatientPanelViewModel PatientsPanel { get; }

    public object? CurrentViewModel => _store.CurrentViewModel;
    public ICommand NavigatePatientsCommand  { get; }

    public HomeViewModel(PatientPanelViewModel patientsPanel, NavigationStore store, NavigationService nav)
    {
        PatientsPanel = patientsPanel;
        _store = store;
        _store.CurrentViewModelChanged += (_, __) => OnPropertyChanged(nameof(CurrentViewModel));

        NavigatePatientsCommand  = new RelayCommand(() => nav.NavigateTo<PatientsViewModel>());
    }
}