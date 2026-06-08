using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public sealed class InvoicesViewModel : ViewModelBase
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private static readonly IBrush DraftBackground = Brush.Parse("#D8F5D0");
    private static readonly IBrush IssuedBackground = Brush.Parse("#FFE7A3");
    private static readonly IBrush OverdueBackground = Brush.Parse("#FFC6B3");
    private static readonly IBrush PayedBackground = Brush.Parse("#D7ECFF");
    private static readonly IBrush CancelledBackground = Brush.Parse("#E3E3E3");

    private readonly BillingService _billingService;
    private readonly IPatientRepository _patientRepository;
    private readonly IInvoicePdfExporter _invoiceExporter;
    private readonly ProjectSession _session;
    private readonly NavigationService _nav;
    private readonly RelayCommand _editDraftCommand;
    private readonly RelayCommand _printInvoiceCommand;

    public ObservableCollection<InvoiceRowVm> Invoices { get; } = new();
    public ObservableCollection<InvoicePositionRowVm> Positions { get; } = new();
    public IReadOnlyList<string> StatusFilters { get; } =
    [
        "Alle",
        nameof(InvoiceStatus.Draft),
        nameof(InvoiceStatus.Issued),
        nameof(InvoiceStatus.Overdue),
        nameof(InvoiceStatus.Payed),
        nameof(InvoiceStatus.Cancelled)
    ];

    public ICommand NavigateHomeViewCommand { get; }
    public ICommand EditDraftCommand => _editDraftCommand;
    public ICommand PrintInvoiceCommand => _printInvoiceCommand;

    private readonly List<Invoice> _allInvoices = new();

    private string _filterText = "";
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText == value) return;
            _filterText = value;
            OnPropertyChanged();
            ReloadInvoices();
        }
    }

    private string _selectedStatusFilter = "Alle";
    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (_selectedStatusFilter == value) return;
            _selectedStatusFilter = value;
            OnPropertyChanged();
            ReloadInvoices();
        }
    }

    private InvoiceRowVm? _selectedInvoice;
    public InvoiceRowVm? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (_selectedInvoice == value) return;
            _selectedInvoice = value;
            OnPropertyChanged();
            RefreshSelectedInvoiceDetails();
            _editDraftCommand.RaiseCanExecuteChanged();
            _printInvoiceCommand.RaiseCanExecuteChanged();
        }
    }

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

    private string _patientDetailsIdAndName = "-";
    public string PatientDetailsIdAndName
    {
        get => _patientDetailsIdAndName;
        private set
        {
            if (_patientDetailsIdAndName == value) return;
            _patientDetailsIdAndName = value;
            OnPropertyChanged();
        }
    }

    private string _patientDateOfBirth = "-";
    public string PatientDateOfBirth
    {
        get => _patientDateOfBirth;
        private set
        {
            if (_patientDateOfBirth == value) return;
            _patientDateOfBirth = value;
            OnPropertyChanged();
        }
    }

    private string _patientInsuranceStatus = "-";
    public string PatientInsuranceStatus
    {
        get => _patientInsuranceStatus;
        private set
        {
            if (_patientInsuranceStatus == value) return;
            _patientInsuranceStatus = value;
            OnPropertyChanged();
        }
    }

    private string _invoiceStatus = "-";
    public string InvoiceStatusText
    {
        get => _invoiceStatus;
        private set
        {
            if (_invoiceStatus == value) return;
            _invoiceStatus = value;
            OnPropertyChanged();
        }
    }

    private IBrush _invoiceStatusBackground = Brushes.Transparent;
    public IBrush InvoiceStatusBackground
    {
        get => _invoiceStatusBackground;
        private set
        {
            if (_invoiceStatusBackground == value) return;
            _invoiceStatusBackground = value;
            OnPropertyChanged();
        }
    }

    private string _invoiceDueDate = "-";
    public string InvoiceDueDate
    {
        get => _invoiceDueDate;
        private set
        {
            if (_invoiceDueDate == value) return;
            _invoiceDueDate = value;
            OnPropertyChanged();
        }
    }

    private string _invoiceTotalAmount = "-";
    public string InvoiceTotalAmount
    {
        get => _invoiceTotalAmount;
        private set
        {
            if (_invoiceTotalAmount == value) return;
            _invoiceTotalAmount = value;
            OnPropertyChanged();
        }
    }

    private string _invoiceNumber = "-";
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        private set
        {
            if (_invoiceNumber == value) return;
            _invoiceNumber = value;
            OnPropertyChanged();
        }
    }

    public InvoicesViewModel(
        BillingService billingService,
        IPatientRepository patientRepository,
        IInvoicePdfExporter invoiceExporter,
        ProjectSession session,
        NavigationService nav)
    {
        _billingService = billingService;
        _patientRepository = patientRepository;
        _invoiceExporter = invoiceExporter;
        _session = session;
        _nav = nav;

        NavigateHomeViewCommand = new RelayCommand(() => _nav.NavigateTo<HomeViewModel>());
        _editDraftCommand = new RelayCommand(EditSelectedDraft, CanEditSelectedDraft);
        _printInvoiceCommand = new RelayCommand(PrintSelectedInvoice, CanPrintSelectedInvoice);

        LoadAllInvoices();
    }

    private void LoadAllInvoices()
    {
        _allInvoices.Clear();
        _allInvoices.AddRange(_billingService.ViewInvoices());
        ReloadInvoices();
    }

    private void ReloadInvoices()
    {
        var selectedInvoiceId = SelectedInvoice?.Invoice.Id;
        Invoices.Clear();

        foreach (var invoice in ApplyFilters(_allInvoices).OrderByDescending(x => x.IssueDate))
        {
            Invoices.Add(ToRow(invoice));
        }

        SelectedInvoice = selectedInvoiceId.HasValue
            ? Invoices.FirstOrDefault(row => row.Invoice.Id == selectedInvoiceId.Value) ?? Invoices.FirstOrDefault()
            : Invoices.FirstOrDefault();

        if (Invoices.Count == 0)
        {
            ClearSelectedInvoiceDetails();
        }
    }

    private IEnumerable<Invoice> ApplyFilters(IEnumerable<Invoice> invoices)
    {
        if (Enum.TryParse<InvoiceStatus>(SelectedStatusFilter, out var status))
        {
            invoices = invoices.Where(invoice => invoice.Status == status);
        }

        var filter = FilterText.Trim();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            invoices = invoices.Where(invoice =>
                invoice.Id.ToString("D").Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                invoice.InvoiceNumber.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                invoice.PatientData.Id.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                invoice.PatientData.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                invoice.Status.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return invoices;
    }

    private InvoiceRowVm ToRow(Invoice invoice)
    {
        return new InvoiceRowVm
        {
            Invoice = invoice,
            Date = FormatDate(invoice.IssueDate),
            InvoiceId = invoice.Id.ToString("D"),
            Patient = $"{invoice.PatientData.Id} - {invoice.PatientData.Name}",
            TotalAmount = FormatCurrency(invoice.TotalAmount),
            Status = invoice.Status.ToString(),
            StatusBackground = GetStatusBackground(invoice.Status)
        };
    }

    private void RefreshSelectedInvoiceDetails()
    {
        Positions.Clear();

        if (SelectedInvoice is null)
        {
            ClearSelectedInvoiceDetails();
            return;
        }

        var invoice = SelectedInvoice.Invoice;
        PatientDetailsIdAndName = $"{invoice.PatientData.Id} - {invoice.PatientData.Name}";
        InvoiceStatusText = invoice.Status.ToString();
        InvoiceStatusBackground = GetStatusBackground(invoice.Status);
        InvoiceDueDate = FormatDate(invoice.DueDate);
        InvoiceTotalAmount = FormatCurrency(invoice.TotalAmount);
        InvoiceNumber = string.IsNullOrWhiteSpace(invoice.InvoiceNumber) ? "-" : invoice.InvoiceNumber;

        TryLoadPatientDetails(invoice.PatientData.Id);

        foreach (var appointment in invoice.AppointmentDataList.OrderBy(x => x.Date))
        {
            Positions.Add(new InvoicePositionRowVm
            {
                Date = FormatDateTime(appointment.Date),
                Position = string.IsNullOrWhiteSpace(appointment.AppointmentId) ? "-" : appointment.AppointmentId,
                BillingNumbers = FormatBillingNumbers(appointment.BillingNumbers),
                Amount = FormatCurrency(appointment.TotalAmount)
            });
        }
    }

    private void TryLoadPatientDetails(string patientId)
    {
        try
        {
            var patient = _patientRepository.GetById(patientId);
            PatientDateOfBirth = patient.DateOfBirth?.ToString("dd.MM.yyyy", GermanCulture) ?? "-";
            PatientInsuranceStatus = patient.InsuranceStatus.ToString();
        }
        catch
        {
            PatientDateOfBirth = "-";
            PatientInsuranceStatus = "-";
        }
    }

    private void ClearSelectedInvoiceDetails()
    {
        PatientDetailsIdAndName = "-";
        PatientDateOfBirth = "-";
        PatientInsuranceStatus = "-";
        InvoiceStatusText = "-";
        InvoiceStatusBackground = Brushes.Transparent;
        InvoiceDueDate = "-";
        InvoiceTotalAmount = "-";
        InvoiceNumber = "-";
        Positions.Clear();
        _editDraftCommand.RaiseCanExecuteChanged();
        _printInvoiceCommand.RaiseCanExecuteChanged();
    }

    private bool CanEditSelectedDraft()
    {
        return SelectedInvoice?.Invoice.Status == InvoiceStatus.Draft;
    }

    private void EditSelectedDraft()
    {
        if (SelectedInvoice is null || SelectedInvoice.Invoice.Status != InvoiceStatus.Draft)
        {
            StatusMessage = "Nur Draft-Rechnungen können bearbeitet werden.";
            return;
        }

        var invoiceId = SelectedInvoice.Invoice.Id;
        _nav.NavigateTo<InvoiceDraftViewModel>(vm => vm.LoadDraft(invoiceId));
    }

    private bool CanPrintSelectedInvoice()
    {
        return SelectedInvoice is not null && SelectedInvoice.Invoice.Status != InvoiceStatus.Draft;
    }

    private void PrintSelectedInvoice()
    {
        if (SelectedInvoice is null)
        {
            StatusMessage = "Bitte zuerst eine Rechnung auswählen.";
            return;
        }

        var invoice = SelectedInvoice.Invoice;
        if (invoice.Status == InvoiceStatus.Draft)
        {
            StatusMessage = "Draft-Rechnungen können noch nicht gedruckt werden.";
            return;
        }

        try
        {
            var exportDirectory = ResolveDefaultPdfExportDirectory();
            Directory.CreateDirectory(exportDirectory);
            var exportPath = Path.Combine(exportDirectory, BuildPdfFileName(invoice));
            _invoiceExporter.Export(invoice, exportPath);
            StatusMessage = $"Rechnung wurde erneut als PDF erstellt: {exportPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Rechnung konnte nicht erneut erstellt werden: {ex.Message}";
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

    private static string BuildPdfFileName(Invoice invoice)
    {
        var identifier = string.IsNullOrWhiteSpace(invoice.InvoiceNumber)
            ? invoice.Id.ToString("D")
            : invoice.InvoiceNumber;
        var safeIdentifier = string.Join("_", identifier.Split(Path.GetInvalidFileNameChars()));
        return $"Invoice_{safeIdentifier}.pdf";
    }

    private static string FormatBillingNumbers(IReadOnlyList<BillingNumber> billingNumbers)
    {
        if (billingNumbers.Count == 0)
            return "-";

        return string.Join(", ", billingNumbers.Select(x => x.NumberIdentifier).Distinct());
    }

    private static IBrush GetStatusBackground(InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Draft => DraftBackground,
            InvoiceStatus.Issued => IssuedBackground,
            InvoiceStatus.Overdue => OverdueBackground,
            InvoiceStatus.Payed => PayedBackground,
            InvoiceStatus.Cancelled => CancelledBackground,
            _ => Brushes.Transparent
        };
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C", GermanCulture);
    }

    private static string FormatDate(DateTime date)
    {
        return date == default ? "-" : date.ToString("dd.MM.yyyy", GermanCulture);
    }

    private static string FormatDateTime(DateTime date)
    {
        return date == default ? "-" : date.ToString("dd.MM.yyyy HH:mm", GermanCulture);
    }

    public sealed class InvoiceRowVm
    {
        public Invoice Invoice { get; init; } = null!;
        public string Date { get; init; } = "";
        public string InvoiceId { get; init; } = "";
        public string Patient { get; init; } = "";
        public string TotalAmount { get; init; } = "";
        public string Status { get; init; } = "";
        public IBrush StatusBackground { get; init; } = Brushes.Transparent;
    }

    public sealed class InvoicePositionRowVm
    {
        public string Date { get; init; } = "";
        public string Position { get; init; } = "";
        public string BillingNumbers { get; init; } = "";
        public string Amount { get; init; } = "";
    }
}
