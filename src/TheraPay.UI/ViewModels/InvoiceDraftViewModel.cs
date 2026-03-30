using System;
using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public sealed class InvoiceDraftViewModel : ViewModelBase
{
    private readonly NavigationService _nav;
    private readonly BillingService _billingService;
    private readonly IPatientRepository _patientRepository;
    private readonly ProjectSession _session;
    private readonly IInvoicePdfExporter _invoiceExporter;
    private Invoice? _currentDraft;
    private bool _issueConfirmationRequired;

    public ObservableCollection<InvoiceAppointmentRowVm> Appointments { get; } = new();
    public RelayCommand NavigateBackCommand { get; }
    public RelayCommand SaveAndNavigateHomeCommand { get; }
    public RelayCommand IssueInvoiceCommand { get; }

    public string IssueActionText => _issueConfirmationRequired ? "Wirklich ausstellen" : "Rechnung ausstellen";
    public string IssueWarningText => _issueConfirmationRequired
        ? "Achtung: Nach dem Ausstellen ist die Rechnung nicht mehr änderbar."
        : "";

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

    private DateTime? _invoiceDate = DateTime.Today;
    public DateTime? InvoiceDate
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
            if (InvoiceDate is null)
                return "-";

            return InvoiceDate.Value.AddDays(PaymentTermInDays).ToString("dd.MM.yyyy");
        }
    }

    private string _invoiceSubject = "";
    public string InvoiceSubject
    {
        get => _invoiceSubject;
        set { if (_invoiceSubject != value) { _invoiceSubject = value; OnPropertyChanged(); } }
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

    public InvoiceDraftViewModel(BillingService billingService, IPatientRepository patientRepository, IInvoicePdfExporter invoiceExporter,ProjectSession session, NavigationService nav)
    {
        _nav = nav;
        _billingService = billingService;
        _patientRepository = patientRepository;
        _session = session;
        _invoiceExporter = invoiceExporter;

        NavigateBackCommand = new RelayCommand(() => _nav.NavigateTo<InvoiceCreationViewModel>());
        SaveAndNavigateHomeCommand = new RelayCommand(SaveAndNavigateHome);
        IssueInvoiceCommand = new RelayCommand(IssueInvoiceWithWarning);

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
        InvoiceSubject = practiceData.Subject ?? "Rechnung";
        PracticeStreet = practiceData.Street;
        PracticeHouseNumber = practiceData.HouseNumber;
        PracticePostalCode = practiceData.PostalCode;
        PracticeCity = practiceData.City;
        PracticeCountry = practiceData.Country ?? "";
        PracticeAddressAdditional = practiceData.AddressAdditional ?? "";
        InvoiceDate = DateTime.Today;
        IncludeSignatureSection = true;
        IncludeQrCode = true;
    }

    private void LoadLatestDraft()
    {
        Appointments.Clear();
        StatusMessage = "";
        DraftId = "-";
        _currentDraft = null;
        _issueConfirmationRequired = false;
        OnPropertyChanged(nameof(IssueActionText));
        OnPropertyChanged(nameof(IssueWarningText));

        var latestDraft = _billingService
            .ViewInvoices()
            .Where(x => x.Status == InvoiceStatus.Draft)
            .LastOrDefault();

        if (latestDraft is null)
        {
            StatusMessage = "Kein Invoice-Draft gefunden.";
            return;
        }

        _currentDraft = latestDraft;
        DraftId = latestDraft.Id.ToString("D");
        var issueDate = latestDraft.IssueDate == default ? DateTime.Today : latestDraft.IssueDate;
        InvoiceDate = issueDate;
        PaymentTermInDays = latestDraft.PracticeDataRecord.DefaultPaymentTermDays;

        PracticeName = latestDraft.PracticeDataRecord.Name;
        PracticeTaxNumber = latestDraft.PracticeDataRecord.TaxNumber;
        PracticeIban = latestDraft.PracticeDataRecord.PaymentDetails.IBAN;
        PracticeBlz = latestDraft.PracticeDataRecord.PaymentDetails.BLZ ?? "";
        PracticeBankName = latestDraft.PracticeDataRecord.PaymentDetails.BankName ?? "";
        InvoiceSubject = latestDraft.PracticeDataRecord.PaymentDetails.Subject ?? "Rechnung";
        PracticeStreet = latestDraft.PracticeDataRecord.Address.Street;
        PracticeHouseNumber = latestDraft.PracticeDataRecord.Address.HouseNumber;
        PracticePostalCode = latestDraft.PracticeDataRecord.Address.PostalCode;
        PracticeCity = latestDraft.PracticeDataRecord.Address.City;
        PracticeCountry = latestDraft.PracticeDataRecord.Address.Country ?? "";
        PracticeAddressAdditional = latestDraft.PracticeDataRecord.Address.Additional ?? "";

        PatientId = latestDraft.PatientData.Id;
        MapPatientNameFromDraft(latestDraft.PatientData.Name);
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
        ApplyPracticeDraftToSession();
        _session.MarkUnsavedChanges();
        StatusMessage = "Draft-Daten gespeichert.";
        _nav.NavigateTo<HomeViewModel>();
    }

    private void IssueInvoiceWithWarning()
    {
        if (_currentDraft is null)
        {
            StatusMessage = "Kein Draft verfuegbar.";
            return;
        }

        if (!_issueConfirmationRequired)
        {
            _issueConfirmationRequired = true;
            OnPropertyChanged(nameof(IssueActionText));
            OnPropertyChanged(nameof(IssueWarningText));
            StatusMessage = "Bitte erneut auf \"Rechnung ausstellen\" klicken, um final auszustellen.";
            return;
        }

        try
        {
            var issueDateForNumber = DateTime.Today;
            var serial = _session.PracticeData.InvoiceNumberState.ConsumeNextSerial(issueDateForNumber);
            var invoiceNumber = $"{issueDateForNumber:yyyyMM}-{serial:0000}";

            var issueResult = _currentDraft.Issue(BuildPracticeDataRecordFromDraft(), invoiceNumber);
            if (!issueResult.Ok)
            {
                StatusMessage = issueResult.Error ?? "Rechnung konnte nicht ausgestellt werden.";
                return;
            }
            
            _invoiceExporter.Export(_currentDraft, $"Invoice_{invoiceNumber}.pdf");

            ApplyPracticeDraftToSession();
            _session.MarkUnsavedChanges();
            _issueConfirmationRequired = false;
            OnPropertyChanged(nameof(IssueActionText));
            OnPropertyChanged(nameof(IssueWarningText));
            StatusMessage = $"Rechnung wurde ausgestellt (Nr.: {invoiceNumber}) und ist jetzt nicht mehr editierbar.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private PracticeDataRecord BuildPracticeDataRecordFromDraft()
    {
        return new PracticeDataRecord
        {
            Name = PracticeName,
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
                string.IsNullOrWhiteSpace(PracticeBankName) ? null : PracticeBankName,
                string.IsNullOrWhiteSpace(InvoiceSubject) ? null : InvoiceSubject),
            DefaultPaymentTermDays = PaymentTermInDays
        };
    }

    private void ApplyPracticeDraftToSession()
    {
        _session.PracticeData.Name = PracticeName;
        _session.PracticeData.TaxIdentificationNumber = PracticeTaxNumber;
        _session.PracticeData.IBAN = PracticeIban;
        _session.PracticeData.BLZ = string.IsNullOrWhiteSpace(PracticeBlz) ? null : PracticeBlz;
        _session.PracticeData.BankName = string.IsNullOrWhiteSpace(PracticeBankName) ? null : PracticeBankName;
        _session.PracticeData.Subject = string.IsNullOrWhiteSpace(InvoiceSubject) ? null : InvoiceSubject;
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
        }
        catch
        {
            // Patientdaten im Draft bleiben erhalten; zusätzliche Felder sind optional editierbar im View.
        }
    }

    public sealed class InvoiceAppointmentRowVm
    {
        public string AppointmentId { get; init; } = "";
        public string Date { get; init; } = "";
        public string Amount { get; init; } = "";
        public string PatientId { get; init; } = "";
    }
}
