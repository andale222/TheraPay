using Avalonia.Controls;
using TheraPay.UI.Views;
using TheraPay.UI.ViewModels;
using TheraPay.Core;
using TheraPay.UI.Navigation;

namespace TheraPay.UI;

public partial class MainWindow : Window
{
    private INavigationService? _navigationService;
    private Control? _originalContent;
    public MainWindow()
    {
        InitializeComponent();
        _navigationService = new NavigationService();
        (_navigationService as NavigationService)?.SetMainWindow(this);
        
        _originalContent = Content as Control;
    }

    public void AddPatient_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NavigateToPatients2();
    }

    public void NavigateToPatients2()
    {
        var store = new InMemoryPatientRepository();                 // dein InMemory (aus Core)
        var vm = new PatientsViewModel(store, _navigationService);

        Content = new PatientsView { DataContext = vm };
    }
    public void NavigateToMain()
    {
        Content = _originalContent;
    }
}