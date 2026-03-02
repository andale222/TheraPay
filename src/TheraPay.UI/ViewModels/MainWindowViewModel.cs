using TheraPay.UI.Navigation;

namespace TheraPay.UI.ViewModels;
public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationStore _store;

    public object? CurrentViewModel => _store.CurrentViewModel;

    public MainWindowViewModel(NavigationStore store, NavigationService nav)
    {
        _store = store;
        _store.CurrentViewModelChanged += (_, __) =>
            OnPropertyChanged(nameof(CurrentViewModel));

        // Startscreen
        nav.NavigateTo<HomeViewModel>();
    }
}