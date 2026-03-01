using Avalonia.Controls;
using Avalonia.Interactivity;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Views;


public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    

    private void AddPatient_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as HomeViewModel)?.NavigatePatientsCommand.Execute(null);
    }
}