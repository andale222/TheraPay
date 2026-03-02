using System;

namespace TheraPay.UI.Navigation;

public sealed class NavigationStore
{
    public object? CurrentViewModel { get; private set; }
    public event EventHandler? CurrentViewModelChanged;

    public void Navigate(object? viewModel)
    {
        CurrentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
    }
}

