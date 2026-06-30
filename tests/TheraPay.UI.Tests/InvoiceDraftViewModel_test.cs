using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Core.Export;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.Services;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Tests;

public class InvoiceDraftViewModel_test
{
    [Fact]
    public void GivenPdfExportDirectory_ExportTestInvoice_ExportsPdfWithTestFileName()
    {
        // GIVEN
        var invoiceRepository = new InMemoryInvoiceRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patientRepository.Add(patient);

        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(60);
        appointmentRepository.Add(appointment);

        var practiceData = new PracticeData
        {
            Name = "Praxis Test",
            Street = "Testweg",
            HouseNumber = "1",
            PostalCode = "12345",
            City = "Teststadt",
            TaxIdentificationNumber = "123",
            IBAN = "DE00 0000 0000 0000 0000 00",
            DefaultPaymentTermDays = 14,
            InvoiceNumberState = InvoiceNumberState.Rehydrate(2026, 1200, 1)
        };

        var billingService = new BillingService(invoiceRepository, appointmentRepository, patientRepository);
        var addResult = billingService.AddInvoiceForPatientAndAppointments(
            patient.ID,
            [appointment.Id],
            practiceData,
            new DateTime(2026, 1, 5),
            14);
        Assert.True(addResult.Ok);

        var exportDirectory = Path.Combine(Path.GetTempPath(), "therapay-pdf-export-test", Guid.NewGuid().ToString("N"));
        var session = new ProjectSession();
        session.SetPracticeData(practiceData);
        var exporter = new CapturingInvoicePdfExporter();
        var messageBox = new ConfirmingMessageBoxService();
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var viewModel = new InvoiceDraftViewModel(
            billingService,
            patientRepository,
            exporter,
            session,
            navigationService,
            messageBox)
        {
            PdfExportDirectory = exportDirectory
        };

        try
        {
            // WHEN
            viewModel.ExportTestInvoiceCommand.Execute(null);

            // THEN
            Assert.NotNull(exporter.LastFilePath);
            Assert.StartsWith(exportDirectory, exporter.LastFilePath);
            Assert.EndsWith(".pdf", exporter.LastFilePath);
            Assert.Equal("Invoice_TEST.pdf", Path.GetFileName(exporter.LastFilePath));
        }
        finally
        {
            if (Directory.Exists(exportDirectory))
            {
                Directory.Delete(exportDirectory, true);
            }
        }
    }

    [Fact]
    public void GivenPdfExportDirectory_IssueInvoice_ExportsPdfToSelectedDirectory()
    {
        // GIVEN
        var invoiceRepository = new InMemoryInvoiceRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patientRepository.Add(patient);

        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(60);
        appointmentRepository.Add(appointment);

        var practiceData = new PracticeData
        {
            Name = "Praxis Test",
            Street = "Testweg",
            HouseNumber = "1",
            PostalCode = "12345",
            City = "Teststadt",
            TaxIdentificationNumber = "123",
            IBAN = "DE00 0000 0000 0000 0000 00",
            DefaultPaymentTermDays = 14,
            InvoiceNumberState = InvoiceNumberState.Rehydrate(2026, 1200, 1)
        };

        var billingService = new BillingService(invoiceRepository, appointmentRepository, patientRepository);
        var addResult = billingService.AddInvoiceForPatientAndAppointments(
            patient.ID,
            [appointment.Id],
            practiceData,
            new DateTime(2026, 1, 5),
            14);
        Assert.True(addResult.Ok);

        var exportDirectory = Path.Combine(Path.GetTempPath(), "therapay-pdf-export-test", Guid.NewGuid().ToString("N"));
        var session = new ProjectSession();
        session.SetPracticeData(practiceData);
        var exporter = new CapturingInvoicePdfExporter();
        var messageBox = new ConfirmingMessageBoxService();
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var viewModel = new InvoiceDraftViewModel(
            billingService,
            patientRepository,
            exporter,
            session,
            navigationService,
            messageBox)
        {
            PdfExportDirectory = exportDirectory
        };

        try
        {
            // WHEN
            viewModel.IssueInvoiceCommand.Execute(null);

            // THEN
            Assert.Equal(1, messageBox.ConfirmationCount);
            Assert.NotNull(exporter.LastFilePath);
            Assert.StartsWith(exportDirectory, exporter.LastFilePath);
            Assert.EndsWith(".pdf", exporter.LastFilePath);
            Assert.Equal($"Invoice_{billingService.ViewInvoices()[0].InvoiceNumber}.pdf", Path.GetFileName(exporter.LastFilePath));
            var exportedInvoice = Assert.IsType<InvoicePdfModel>(exporter.LastInvoice);
            Assert.True(exportedInvoice.IncludePaymentQrCode);
        }
        finally
        {
            if (Directory.Exists(exportDirectory))
            {
                Directory.Delete(exportDirectory, true);
            }
        }
    }

    private sealed class CapturingInvoicePdfExporter : IInvoicePdfExporter
    {
        public string? LastFilePath { get; private set; }
        public InvoicePdfModel? LastInvoice { get; private set; }

        public bool InternalExport(InvoicePdfModel invoice, string filePath)
        {
            LastFilePath = filePath;
            LastInvoice = invoice;
            return true;
        }
    }

    private sealed class ConfirmingMessageBoxService : IMessageBoxService
    {
        public int ConfirmationCount { get; private set; }

        public Task ShowErrorAsync(string title, string message)
        {
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmWarningAsync(string title, string message, string confirmText = "OK", string cancelText = "Abbrechen")
        {
            ConfirmationCount++;
            return Task.FromResult(true);
        }
    }
}
