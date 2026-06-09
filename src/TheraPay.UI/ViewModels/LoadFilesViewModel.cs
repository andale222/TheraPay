using TheraPay.UI.Navigation;
using TheraPay.UI.Services;
using TheraPay.Infrastructure.csv;
using TheraPay.Domain;

namespace TheraPay.UI.ViewModels;

public sealed class LoadFilesViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly ProjectPersistenceService _projectPersistence;

    public RelayCommand LoadCommand { get; }
    public RelayCommand StartEmptyProjectCommand { get; }
    public RelayCommand EncryptPlaintextDatabasesCommand { get; }
    public RelayCommand DecryptEncryptedDatabasesCommand { get; }

    private string _patientListPath = "";
    public string PatientListPath
    {
        get => _patientListPath;
        set
        {
            if (_patientListPath == value) return;
            _patientListPath = value;
            OnPropertyChanged();
        }
    }

    private string _appointmentListPath = "";
    public string AppointmentListPath
    {
        get => _appointmentListPath;
        set
        {
            if (_appointmentListPath == value) return;
            _appointmentListPath = value;
            OnPropertyChanged();
        }
    }

    private string _invoiceListPath = "";
    public string InvoiceListPath
    {
        get => _invoiceListPath;
        set
        {
            if (_invoiceListPath == value) return;
            _invoiceListPath = value;
            OnPropertyChanged();
        }
    }

    private string _practiceDataPath = "";
    public string PracticeDataPath
    {
        get => _practiceDataPath;
        set
        {
            if (_practiceDataPath == value) return;
            _practiceDataPath = value;
            OnPropertyChanged();
        }
    }

    private bool _useEncryption;
    public bool UseEncryption
    {
        get => _useEncryption;
        set
        {
            if (_useEncryption == value) return;
            _useEncryption = value;
            OnPropertyChanged();
        }
    }

    private string _encryptionPassword = "";
    public string EncryptionPassword
    {
        get => _encryptionPassword;
        set
        {
            if (_encryptionPassword == value) return;
            _encryptionPassword = value;
            OnPropertyChanged();
        }
    }

    private string _conversionOutputDirectory = "";
    public string ConversionOutputDirectory
    {
        get => _conversionOutputDirectory;
        set
        {
            if (_conversionOutputDirectory == value) return;
            _conversionOutputDirectory = value;
            OnPropertyChanged();
        }
    }

    private string _conversionPassword = "";
    public string ConversionPassword
    {
        get => _conversionPassword;
        set
        {
            if (_conversionPassword == value) return;
            _conversionPassword = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public LoadFilesViewModel(NavigationService nav, ProjectPersistenceService projectPersistence)
    {
        _nav = nav;
        _projectPersistence = projectPersistence;
        _useEncryption = true;

        LoadCommand = new RelayCommand(LoadProject);
        StartEmptyProjectCommand = new RelayCommand(StartEmptyProject);
        EncryptPlaintextDatabasesCommand = new RelayCommand(EncryptPlaintextDatabases);
        DecryptEncryptedDatabasesCommand = new RelayCommand(DecryptEncryptedDatabases);
    }

    private void LoadProject()
    {
        if (!TryCreateFileEncryption(out var fileEncryption))
        {
            return;
        }

        var result = _projectPersistence.LoadProject(
            PatientListPath,
            AppointmentListPath,
            PracticeDataPath,
            InvoiceListPath,
            fileEncryption);

        if (!result.Ok)
        {
            StatusMessage = result.Error ?? "Laden fehlgeschlagen.";
            return;
        }

        StatusMessage = "";
        _nav.NavigateTo<HomeViewModel>();
    }

    private void StartEmptyProject()
    {
        if (!TryCreateFileEncryption(out var fileEncryption))
        {
            return;
        }

        _projectPersistence.StartEmptyProject(
            PatientListPath,
            AppointmentListPath,
            PracticeDataPath,
            InvoiceListPath,
            fileEncryption);

        StatusMessage = "";
        _nav.NavigateTo<HomeViewModel>();
    }

    private bool TryCreateFileEncryption(out ICsvFileEncryption? fileEncryption)
    {
        fileEncryption = null;

        if (!UseEncryption)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(EncryptionPassword))
        {
            StatusMessage = "Bitte gib ein Passwort fuer die verschluesselten CSV-Dateien ein.";
            return false;
        }

        fileEncryption = new AesGcmCsvFileEncryption(EncryptionPassword);
        return true;
    }

    private void EncryptPlaintextDatabases()
    {
        SetConversionStatus(_projectPersistence.EncryptProjectFiles(
            PatientListPath,
            AppointmentListPath,
            PracticeDataPath,
            InvoiceListPath,
            ConversionOutputDirectory,
            ConversionPassword), "Plaintext-Datenbanken wurden verschluesselt gespeichert.");
    }

    private void DecryptEncryptedDatabases()
    {
        SetConversionStatus(_projectPersistence.DecryptProjectFiles(
            PatientListPath,
            AppointmentListPath,
            PracticeDataPath,
            InvoiceListPath,
            ConversionOutputDirectory,
            ConversionPassword), "Verschluesselte Datenbanken wurden als Plaintext gespeichert.");
    }

    private void SetConversionStatus(Result result, string successMessage)
    {
        StatusMessage = result.Ok
            ? successMessage
            : result.Error ?? "Konvertierung fehlgeschlagen.";
    }
}
