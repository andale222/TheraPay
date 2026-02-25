using Avalonia.Controls;
using TheraPay.UI.Views;
using TheraPay.UI.ViewModels;
using TheraPay.Domain;

namespace TheraPay.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


        var store = new InMemoryPatientRepository();                 // dein InMemory (aus Core)
        var vm = new PatientsViewModel(store);

        Content = new PatientsView { DataContext = vm };
    }
}