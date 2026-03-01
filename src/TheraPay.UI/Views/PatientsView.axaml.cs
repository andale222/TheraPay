using Avalonia.Controls;
using Avalonia.Interactivity;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Views;

public partial class PatientsView : UserControl
{
    public PatientsView() => InitializeComponent();

    private void AddPatient_Click(object? sender, RoutedEventArgs e)
    {
        (DataContext as PatientsViewModel)?.AddPatient();
        (DataContext as PatientsViewModel)?.GoBack();
    }

    private void CheckData_Click(object? sender, RoutedEventArgs e)
    {
        // var vm = DataContext as PatientsViewModel;
        // if (vm != null)
        // {
        //     string message = $"First Name: {vm.FirstName}\nLast Name: {vm.LastName}\nPatient ID: {vm.PatientID}";
        //     MessageBox.Show(message, "Patient Data");
        // }
    }
}