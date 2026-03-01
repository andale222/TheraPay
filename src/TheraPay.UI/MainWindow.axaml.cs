using Avalonia.Controls;
using TheraPay.UI.Views;
using TheraPay.UI.ViewModels;
using TheraPay.Core;
using TheraPay.UI.Navigation;

namespace TheraPay.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}