using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.Services;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public sealed class InvoiceDraftViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly BillingService _billingService;
    private readonly IPatientRepository _patientRepository;
    private readonly ProjectSession _session;
    private readonly IInvoicePdfExporter _invoiceExporter;
    private readonly IMessageBoxService _messageBox;
    private Invoice? _currentDraft;

    public ObservableCollection<InvoiceAppointmentRowVm> Appointments { get; } = new();
    public RelayCommand NavigateBackCommand { get; }
    public RelayCommand NavigateHomeWithoutSavingCommand { get; }
    public RelayCommand SaveAndNavigateHomeCommand { get; }
    public RelayCommand IssueInvoiceCommand { get; }
    public RelayCommand ExportTestInvoiceCommand { get; }

    public string IssueActionText => "Rechnung ausstellen";

    public string PdfExportFilePreview => Path.Combine(PdfExportDirectory, "Invoice_<Rechnungsnummer>.pdf");

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    private string _draftId = "-";
    public string DraftId
    {
        get => _draftId;
        private set
        {
            if (_draftId == value) return;
            _draftId = value;
            OnPropertyChanged();
        }
    }

    private DateTime _invoiceDate = DateTime.Today;
    public DateTime InvoiceDate
    {
        get => _invoiceDate;
        set
        {
            if (_invoiceDate == value) return;
            _invoiceDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DueDatePreview));
        }
    }

    private int _paymentTermInDays = 14;
    public int PaymentTermInDays
    {
        get => _paymentTermInDays;
        set
        {
            if (_paymentTermInDays == value) return;
            _paymentTermInDays = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DueDatePreview));
        }
    }

    public string DueDatePreview
    {
        get
        {
            return InvoiceDate.AddDays(PaymentTermInDays).ToString("dd.MM.yyyy");
        }
    }

    private string _invoiceSubject = "";
    public string InvoiceSubject
    {
        get => _invoiceSubject;
        set { if (_invoiceSubject != value) { _invoiceSubject = value; OnPropertyChanged(); } }
    }

    private string _pdfExportDirectory = "";
    public string PdfExportDirectory
    {
        get => _pdfExportDirectory;
        set
        {
            var normalized = value?.Trim() ?? "";
            if (_pdfExportDirectory == normalized) return;
            _pdfExportDirectory = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PdfExportFilePreview));
        }
    }

    private bool _includeSignatureSection = true;
    public bool IncludeSignatureSection
    {
        get => _includeSignatureSection;
        set { if (_includeSignatureSection != value) { _includeSignatureSection = value; OnPropertyChanged(); } }
    }

    private bool _includeQrCode = true;
    public bool IncludeQrCode
    {
        get => _includeQrCode;
        set { if (_includeQrCode != value) { _includeQrCode = value; OnPropertyChanged(); } }
    }

    private bool _isPracticeDataEditable;
    public bool IsPracticeDataEditable
    {
        get => _isPracticeDataEditable;
        set { if (_isPracticeDataEditable != value) { _isPracticeDataEditable = value; OnPropertyChanged(); } }
    }

    private bool _isPatientDataEditable;
    public bool IsPatientDataEditable
    {
        get => _isPatientDataEditable;
        set { if (_isPatientDataEditable != value) { _isPatientDataEditable = value; OnPropertyChanged(); } }
    }

    private bool _isTransferDataEditable;
    public bool IsTransferDataEditable
    {
        get => _isTransferDataEditable;
        set { if (_isTransferDataEditable != value) { _isTransferDataEditable = value; OnPropertyChanged(); } }
    }

    private string _practiceName = "";
    public string PracticeName
    {
        get => _practiceName;
        set { if (_practiceName != value) { _practiceName = value; OnPropertyChanged(); } }
    }

    private string _practiceTaxNumber = "";
    public string PracticeTaxNumber
    {
        get => _practiceTaxNumber;
        set { if (_practiceTaxNumber != value) { _practiceTaxNumber = value; OnPropertyChanged(); } }
    }

    private string _practiceIban = "";
    public string PracticeIban
    {
        get => _practiceIban;
        set { if (_practiceIban != value) { _practiceIban = value; OnPropertyChanged(); } }
    }

    private string _practiceBlz = "";
    public string PracticeBlz
    {
        get => _practiceBlz;
        set { if (_practiceBlz != value) { _practiceBlz = value; OnPropertyChanged(); } }
    }

    private string _practiceBankName = "";
    public string PracticeBankName
    {
        get => _practiceBankName;
        set { if (_practiceBankName != value) { _practiceBankName = value; OnPropertyChanged(); } }
    }

    private string _practiceStreet = "";
    public string PracticeStreet
    {
        get => _practiceStreet;
        set { if (_practiceStreet != value) { _practiceStreet = value; OnPropertyChanged(); } }
    }

    private string _practiceHouseNumber = "";
    public string PracticeHouseNumber
    {
        get => _practiceHouseNumber;
        set { if (_practiceHouseNumber != value) { _practiceHouseNumber = value; OnPropertyChanged(); } }
    }

    private string _practicePostalCode = "";
    public string PracticePostalCode
    {
        get => _practicePostalCode;
        set { if (_practicePostalCode != value) { _practicePostalCode = value; OnPropertyChanged(); } }
    }

    private string _practiceCity = "";
    public string PracticeCity
    {
        get => _practiceCity;
        set { if (_practiceCity != value) { _practiceCity = value; OnPropertyChanged(); } }
    }

    private string _practiceCountry = "";
    public string PracticeCountry
    {
        get => _practiceCountry;
        set { if (_practiceCountry != value) { _practiceCountry = value; OnPropertyChanged(); } }
    }

    private string _practiceAddressAdditional = "";
    public string PracticeAddressAdditional
    {
        get => _practiceAddressAdditional;
        set { if (_practiceAddressAdditional != value) { _practiceAddressAdditional = value; OnPropertyChanged(); } }
    }

    private string _patientId = "";
    public string PatientId
    {
        get => _patientId;
        set { if (_patientId != value) { _patientId = value; OnPropertyChanged(); } }
    }

    private string _patientFirstName = "";
    public string PatientFirstName
    {
        get => _patientFirstName;
        set { if (_patientFirstName != value) { _patientFirstName = value; OnPropertyChanged(); } }
    }

    private string _patientLastName = "";
    public string PatientLastName
    {
        get => _patientLastName;
        set { if (_patientLastName != value) { _patientLastName = value; OnPropertyChanged(); } }
    }

    private string _patientStreet = "";
    public string PatientStreet
    {
        get => _patientStreet;
        set { if (_patientStreet != value) { _patientStreet = value; OnPropertyChanged(); } }
    }

    private string _patientHouseNumber = "";
    public string PatientHouseNumber
    {
        get => _patientHouseNumber;
        set { if (_patientHouseNumber != value) { _patientHouseNumber = value; OnPropertyChanged(); } }
    }

    private string _patientPostalCode = "";
    public string PatientPostalCode
    {
        get => _patientPostalCode;
        set { if (_patientPostalCode != value) { _patientPostalCode = value; OnPropertyChanged(); } }
    }

    private string _patientCity = "";
    public string PatientCity
    {
        get => _patientCity;
        set { if (_patientCity != value) { _patientCity = value; OnPropertyChanged(); } }
    }

    private string _patientCountry = "";
    public string PatientCountry
    {
        get => _patientCountry;
        set { if (_patientCountry != value) { _patientCountry = value; OnPropertyChanged(); } }
    }

    private string _patientAddressAdditional = "";
    public string PatientAddressAdditional
    {
        get => _patientAddressAdditional;
        set { if (_patientAddressAdditional != value) { _patientAddressAdditional = value; OnPropertyChanged(); } }
    }

    public InvoiceDraftViewModel(
        BillingService billingService,
        IPatientRepository patientRepository,
        IInvoicePdfExporter invoiceExporter,
        ProjectSession session,
        NavigationService nav,
        IMessageBoxService messageBox)
    {
        _nav = nav;
        _billingService = billingService;
        _patientRepository = patientRepository;
        _session = session;
        _invoiceExporter = invoiceExporter;
        _messageBox = messageBox;

        NavigateBackCommand = new RelayCommand(() => _nav.NavigateTo<InvoiceCreationViewModel>());
        NavigateHomeWithoutSavingCommand = new RelayCommand(() => _nav.NavigateTo<HomeViewModel>());
        SaveAndNavigateHomeCommand = new RelayCommand(SaveAndNavigateHome);
        IssueInvoiceCommand = new RelayCommand(async () => await IssueInvoiceWithWarningAsync());
        ExportTestInvoiceCommand = new RelayCommand(async () => await ExportTestInvoiceAsync());

        LoadDefaultsFromSession();
        LoadLatestDraft();
    }

    private void LoadDefaultsFromSession()
    {
        var practiceData = _session.PracticeData;
        PaymentTermInDays = practiceData.DefaultPaymentTermDays;
        PracticeName = practiceData.Name;
        PracticeTaxNumber = practiceData.TaxIdentificationNumber;
        PracticeIban = practiceData.IBAN;
        PracticeBlz = practiceData.BLZ ?? "";
        PracticeBankName = practiceData.BankName ?? "";
        InvoiceSubject = Invoice.DefaultSubject;
        PracticeStreet = practiceData.Street;
        PracticeHouseNumber = practiceData.HouseNumber;
        PracticePostalCode = practiceData.PostalCode;
        PracticeCity = practiceData.City;
        PracticeCountry = practiceData.Country ?? "";
        PracticeAddressAdditional = practiceData.AddressAdditional ?? "";
        InvoiceDate = DateTime.Today;
        IncludeSignatureSection = true;
        IncludeQrCode = true;
        PdfExportDirectory = ResolveDefaultPdfExportDirectory();
    }

    public void SetPdfExportDirectory(string folderPath)
    {
        PdfExportDirectory = folderPath;
        StatusMessage = "PDF-Exportordner übernommen.";
    }

    public void ReportPdfExportDirectorySelectionError(string message)
    {
        _ = _messageBox.ShowErrorAsync("Ordnerauswahl fehlgeschlagen", message);
    }

    public void LoadDraft(Guid invoiceId)
    {
        var draft = _billingService
            .ViewInvoices()
            .FirstOrDefault(x => x.Id == invoiceId && x.Status == InvoiceStatus.Draft);

        if (draft is null)
        {
            ClearDraftSelection();
            _ = _messageBox.ShowWarningAsync(
                "Invoice-Draft",
                $"Draft mit ID '{invoiceId:D}' wurde nicht gefunden.");
            return;
        }

        LoadDraftInvoice(draft);
    }

    private void LoadLatestDraft()
    {
        var latestDraft = _billingService
            .ViewInvoices()
            .Where(x => x.Status == InvoiceStatus.Draft)
            .LastOrDefault();

        if (latestDraft is null)
        {
            ClearDraftSelection();
            _ = _messageBox.ShowWarningAsync(
                "Invoice-Draft",
                "Kein Invoice-Draft gefunden.");
            return;
        }

        LoadDraftInvoice(latestDraft);
    }

    private void ClearDraftSelection()
    {
        Appointments.Clear();
        StatusMessage = "";
        DraftId = "-";
        _currentDraft = null;
        OnPropertyChanged(nameof(IssueActionText));
    }

    private void LoadDraftInvoice(Invoice latestDraft)
    {
        ClearDraftSelection();
        _currentDraft = latestDraft;
        DraftId = latestDraft.Id.ToString("D");
        var issueDate = latestDraft.IssueDate == default ? DateTime.Today : latestDraft.IssueDate;
        InvoiceDate = issueDate;
        PaymentTermInDays = latestDraft.PracticeDataRecord.DefaultPaymentTermDays;

        PracticeName = latestDraft.PracticeDataRecord.PracticeName;
        PracticeTaxNumber = latestDraft.PracticeDataRecord.TaxNumber;
        PracticeIban = latestDraft.PracticeDataRecord.PaymentDetails.IBAN;
        PracticeBlz = latestDraft.PracticeDataRecord.PaymentDetails.BLZ ?? "";
        PracticeBankName = latestDraft.PracticeDataRecord.PaymentDetails.BankName ?? "";
        InvoiceSubject = latestDraft.Subject;
        PracticeStreet = latestDraft.PracticeDataRecord.Address.Street;
        PracticeHouseNumber = latestDraft.PracticeDataRecord.Address.HouseNumber;
        PracticePostalCode = latestDraft.PracticeDataRecord.Address.PostalCode;
        PracticeCity = latestDraft.PracticeDataRecord.Address.City;
        PracticeCountry = latestDraft.PracticeDataRecord.Address.Country ?? "";
        PracticeAddressAdditional = latestDraft.PracticeDataRecord.Address.Additional ?? "";

        PatientId = latestDraft.PatientData.Id;
        MapPatientNameFromDraft(latestDraft.PatientData.Name);
        MapPatientAddressFromDraft(latestDraft.PatientData);
        TryLoadPatientDataFromRepository(latestDraft.PatientData.Id);

        foreach (var appointment in latestDraft.AppointmentDataList.OrderBy(x => x.Date))
        {
            Appointments.Add(new InvoiceAppointmentRowVm
            {
                AppointmentId = appointment.AppointmentId,
                Date = appointment.Date.ToString("dd.MM.yy HH:mm"),
                Amount = appointment.TotalAmount.ToString("0.00"),
                PatientId = appointment.PatientId
            });
        }

        StatusMessage = $"Draft geladen: {Appointments.Count} Termin(e) ausgewählt.";
    }

    private void SaveAndNavigateHome()
    {
        ApplyInvoiceDraftDetails();
        ApplyPracticeDraftToSession();
        _session.MarkUnsavedChanges();
        StatusMessage = "Draft-Daten gespeichert.";
        _nav.NavigateTo<HomeViewModel>();
    }

    private async Task ExportTestInvoiceAsync()
    {
        if (_currentDraft is null)
        {
            await _messageBox.ShowErrorAsync("Test-Rechnung exportieren", "Kein Draft verfuegbar.");
            return;
        }

        try
        {
            var exportDirectory = await TryPreparePdfExportDirectoryAsync();
            if (exportDirectory is null)
            {
                return;
            }

            ApplyInvoiceDraftDetails();
            ApplyPracticeDraftToSession();

            var testInvoiceNumber = "TEST";
            var exportPath = Path.Combine(exportDirectory, BuildPdfFileName(testInvoiceNumber));
            _invoiceExporter.Export(_currentDraft, exportPath, IncludeQrCode);

            _session.MarkUnsavedChanges();
            StatusMessage = $"Test-Rechnung wurde als PDF exportiert: {exportPath}";
        }
        catch (Exception ex)
        {
            await _messageBox.ShowErrorAsync("Test-Rechnung konnte nicht exportiert werden", ex.Message);
        }
    }

    private async Task IssueInvoiceWithWarningAsync()
    {
        if (_currentDraft is null)
        {
            await _messageBox.ShowErrorAsync("Rechnung ausstellen", "Kein Draft verfuegbar.");
            return;
        }

        var confirmed = await _messageBox.ConfirmWarningAsync(
            "Rechnung ausstellen",
            "Achtung: Nach dem Ausstellen ist die Rechnung nicht mehr aenderbar.",
            "Ausstellen",
            "Abbrechen");
        if (!confirmed)
            return;

        try
        {
            var exportDirectory = await TryPreparePdfExportDirectoryAsync();
            if (exportDirectory is null)
            {
                return;
            }

            ApplyInvoiceDraftDetails();
            ApplyPracticeDraftToSession();

            // var invoiceNumber = _session.PracticeData.InvoiceNumberState.PreviewNextSerial(_invoiceDate);
            var issueingResult = _billingService.IssueInvoice(_currentDraft,_invoiceDate,_session.PracticeData);

            // var issueResult = _currentDraft.Issue(BuildPracticeDataRecordFromDraft(), invoiceNumber);
            if (!issueingResult.Ok)
            {
                await _messageBox.ShowErrorAsync(
                    "Rechnung konnte nicht ausgestellt werden",
                    issueingResult.Error ?? "Rechnung konnte nicht ausgestellt werden.");
                return;
            }
            var invoiceNumber = issueingResult.Error ?? "Unbekannt";
            var exportPath = Path.Combine(exportDirectory, BuildPdfFileName(invoiceNumber));

            _invoiceExporter.Export(_currentDraft, exportPath, IncludeQrCode);

            ApplyPracticeDraftToSession();
            _session.MarkUnsavedChanges();
            OnPropertyChanged(nameof(IssueActionText));
            StatusMessage = $"Rechnung wurde ausgestellt (Nr.: {invoiceNumber}), ist jetzt nicht mehr editierbar und wurde als PDF exportiert: {exportPath}";
        }
        catch (Exception ex)
        {
            await _messageBox.ShowErrorAsync("Rechnung konnte nicht ausgestellt werden", ex.Message);
        }
    }

    private async Task<string?> TryPreparePdfExportDirectoryAsync()
    {
        var exportDirectory = PdfExportDirectory.Trim();
        if (string.IsNullOrWhiteSpace(exportDirectory))
        {
            await _messageBox.ShowWarningAsync(
                "PDF-Exportordner fehlt",
                "Bitte einen PDF-Exportordner auswaehlen.");
            return null;
        }

        try
        {
            Directory.CreateDirectory(exportDirectory);
            return exportDirectory;
        }
        catch (Exception ex)
        {
            await _messageBox.ShowErrorAsync(
                "PDF-Exportordner konnte nicht vorbereitet werden",
                ex.Message);
            return null;
        }
    }

    private string ResolveDefaultPdfExportDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_session.PracticeDataPath))
        {
            var projectDirectory = Path.GetDirectoryName(_session.PracticeDataPath);
            if (!string.IsNullOrWhiteSpace(projectDirectory))
            {
                return projectDirectory;
            }
        }

        var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return string.IsNullOrWhiteSpace(documentsDirectory)
            ? AppContext.BaseDirectory
            : documentsDirectory;
    }

    private static string BuildPdfFileName(string invoiceNumber)
    {
        var safeInvoiceNumber = string.Join("_", invoiceNumber.Split(Path.GetInvalidFileNameChars()));
        return $"Invoice_{safeInvoiceNumber}.pdf";
    }

    private PracticeDataRecord BuildPracticeDataRecordFromDraft()
    {
        return new PracticeDataRecord
        {
            PracticeName = PracticeName,
            PracticeDescription = _session.PracticeData.PracticeDescription,
            PracticePhoneNr = _session.PracticeData.PhoneNumber,
            PracticeEmail = _session.PracticeData.PracticeEmail,
            PractitionerFirstLastName = _session.PracticeData.FirstNamePractitioner + " " + _session.PracticeData.LastNamePractitioner,
            Address = new Address(
                PracticeStreet,
                PracticeHouseNumber,
                PracticePostalCode,
                PracticeCity,
                string.IsNullOrWhiteSpace(PracticeCountry) ? null : PracticeCountry,
                string.IsNullOrWhiteSpace(PracticeAddressAdditional) ? null : PracticeAddressAdditional),
            TaxNumber = PracticeTaxNumber,
            PaymentDetails = new PaymentDetails(
                PracticeIban,
                string.IsNullOrWhiteSpace(PracticeBlz) ? null : PracticeBlz,
                string.IsNullOrWhiteSpace(PracticeBankName) ? null : PracticeBankName),
            DefaultPaymentTermDays = PaymentTermInDays
        };
    }

    private void ApplyInvoiceDraftDetails()
    {
        if (_currentDraft is null)
        {
            return;
        }

        _currentDraft.SetDraftDetails(InvoiceDate, PaymentTermInDays, _currentDraft.AdditionalText, InvoiceSubject);
    }

    private void ApplyPracticeDraftToSession()
    {
        _session.PracticeData.Name = PracticeName;
        _session.PracticeData.TaxIdentificationNumber = PracticeTaxNumber;
        _session.PracticeData.IBAN = PracticeIban;
        _session.PracticeData.BLZ = string.IsNullOrWhiteSpace(PracticeBlz) ? null : PracticeBlz;
        _session.PracticeData.BankName = string.IsNullOrWhiteSpace(PracticeBankName) ? null : PracticeBankName;
        _session.PracticeData.Street = PracticeStreet;
        _session.PracticeData.HouseNumber = PracticeHouseNumber;
        _session.PracticeData.PostalCode = PracticePostalCode;
        _session.PracticeData.City = PracticeCity;
        _session.PracticeData.Country = string.IsNullOrWhiteSpace(PracticeCountry) ? null : PracticeCountry;
        _session.PracticeData.AddressAdditional = string.IsNullOrWhiteSpace(PracticeAddressAdditional) ? null : PracticeAddressAdditional;
        _session.PracticeData.DefaultPaymentTermDays = PaymentTermInDays;
    }

    private void MapPatientNameFromDraft(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            PatientFirstName = "";
            PatientLastName = "";
            return;
        }

        if (parts.Length == 1)
        {
            PatientFirstName = "";
            PatientLastName = parts[0];
            return;
        }

        PatientLastName = parts[^1];
        PatientFirstName = string.Join(" ", parts.Take(parts.Length - 1));
    }

    private void TryLoadPatientDataFromRepository(string patientId)
    {
        try
        {
            var patient = _patientRepository.GetById(patientId);
            PatientFirstName = patient.FirstName;
            PatientLastName = patient.LastName;
            if (patient.Address is not null)
            {
                PatientStreet = patient.Address.Street;
                PatientHouseNumber = patient.Address.HouseNumber;
                PatientPostalCode = patient.Address.PostalCode;
                PatientCity = patient.Address.City;
                PatientCountry = patient.Address.Country ?? "";
                PatientAddressAdditional = patient.Address.Additional ?? "";
            }
        }
        catch
        {
            // Patientdaten im Draft bleiben erhalten; zusätzliche Felder sind optional editierbar im View.
        }
    }

    private void MapPatientAddressFromDraft(InvoicePatientData patientData)
    {
        PatientStreet = patientData.Street;
        PatientHouseNumber = patientData.HouseNumber;
        PatientPostalCode = patientData.PostalCode;
        PatientCity = patientData.City;
        PatientCountry = patientData.Country;
        PatientAddressAdditional = patientData.AddressAdditional;
    }

    public sealed class InvoiceAppointmentRowVm
    {
        public string AppointmentId { get; init; } = "";
        public string Date { get; init; } = "";
        public string Amount { get; init; } = "";
        public string PatientId { get; init; } = "";
    }
}
