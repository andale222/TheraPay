using System.Windows.Input;
using TheraPay.UI.Navigation;
using CommunityToolkit.Mvvm.Input;

namespace TheraPay.UI.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly NavigationStore _store;

    public object? CurrentViewModel => _store.CurrentViewModel;
    public ICommand NavigatePatientsCommand  { get; }

    public HomeViewModel(NavigationStore store, NavigationService nav)
    {
        _store = store;
        _store.CurrentViewModelChanged += (_, __) => OnPropertyChanged(nameof(CurrentViewModel));

        NavigatePatientsCommand  = new RelayCommand(() => nav.NavigateTo<PatientsViewModel>());
    }
}