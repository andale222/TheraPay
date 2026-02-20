using Avalonia.Controls;
using Avalonia.Interactivity;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Views;

public partial class PatientsView : UserControl
{
    public PatientsView() => InitializeComponent();

    private void Add_Click(object? sender, RoutedEventArgs e)
        => (DataContext as PatientsViewModel)?.AddPatient();
}