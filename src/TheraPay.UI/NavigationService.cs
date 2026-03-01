using Avalonia.Controls;
using TheraPay.UI.Views;

namespace TheraPay.UI.Navigation;

public class NavigationService : INavigationService
{
    private Window? _mainWindow;

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    public void NavigateToPatients()
    {
        if (_mainWindow is MainWindow mainWindow)
            mainWindow.NavigateToPatients2();
    }

    public void NavigateToMain()
    {
        if (_mainWindow is MainWindow mainWindow)
            mainWindow.NavigateToMain();
    }
}