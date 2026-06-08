using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Views;

public partial class LoadFilesView : UserControl
{
    public LoadFilesView() => InitializeComponent();

    private async void OpenProjectFolderClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not LoadFilesViewModel vm)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            vm.StatusMessage = "Ordnerauswahl wird auf dieser Plattform nicht unterstützt.";
            return;
        }



        var initialPath = AppContext.BaseDirectory;
        IStorageFolder? start = null;
        if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
        {
            start = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialPath);
        }
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Projektordner auswählen",
            AllowMultiple = false,
            SuggestedStartLocation = start
        });

        if (folders.Count == 0)
        {
            return;
        }

        var folderPath = folders[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            vm.StatusMessage = "Der ausgewählte Ordner hat keinen lokalen Dateipfad.";
            return;
        }

        vm.PatientListPath = Path.Combine(folderPath, "patients.csv");
        vm.AppointmentListPath = Path.Combine(folderPath, "appointments.csv");
        vm.InvoiceListPath = Path.Combine(folderPath, "invoices.csv");
        vm.PracticeDataPath = Path.Combine(folderPath, "practice.csv");
        vm.StatusMessage = "Projektordner übernommen.";
    }
}
