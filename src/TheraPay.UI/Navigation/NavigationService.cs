using System;
using Microsoft.Extensions.DependencyInjection;

namespace TheraPay.UI.Navigation;

public sealed class NavigationService
{
    private readonly NavigationStore _store;
    private readonly IServiceProvider _sp;

    public NavigationService(NavigationStore store, IServiceProvider sp)
    {
        _store = store;
        _sp = sp;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var vm = _sp.GetRequiredService<TViewModel>();
        _store.Navigate(vm);
    }

    public void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : class
    {
        var vm = _sp.GetRequiredService<TViewModel>();
        configure(vm);
        _store.Navigate(vm);
    }
}
