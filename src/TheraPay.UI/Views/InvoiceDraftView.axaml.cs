using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Views;

public partial class InvoiceDraftView : UserControl
{
    public InvoiceDraftView() => InitializeComponent();

    private async void SelectPdfExportDirectoryClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InvoiceDraftViewModel vm)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            vm.ReportPdfExportDirectorySelectionError("Ordnerauswahl wird auf dieser Plattform nicht unterstützt.");
            return;
        }

        IStorageFolder? start = null;
        if (!string.IsNullOrWhiteSpace(vm.PdfExportDirectory) && Directory.Exists(vm.PdfExportDirectory))
        {
            start = await topLevel.StorageProvider.TryGetFolderFromPathAsync(vm.PdfExportDirectory);
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "PDF-Exportordner auswählen",
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
            vm.ReportPdfExportDirectorySelectionError("Der ausgewählte Ordner hat keinen lokalen Dateipfad.");
            return;
        }

        vm.SetPdfExportDirectory(folderPath);
    }
}
