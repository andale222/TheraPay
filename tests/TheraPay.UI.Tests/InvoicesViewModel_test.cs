using Microsoft.Extensions.DependencyInjection;
using Avalonia.Media;
using TheraPay.Core;
using TheraPay.Core.Export;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Tests;

public class InvoicesViewModel_test
{
    [Fact]
    public void GivenDraftInvoice_OverviewShowsInvoiceAndOnlyAllowsEditing()
    {
        // GIVEN
        var setup = CreateSetup();
        var viewModel = new InvoicesViewModel(
            setup.BillingService,
            setup.PatientRepository,
            setup.Exporter,
            setup.Session,
            setup.NavigationService);

        // THEN
        Assert.Single(viewModel.Invoices);
        Assert.NotNull(viewModel.SelectedInvoice);
        Assert.Equal("Draft", viewModel.SelectedInvoice!.Status);
        Assert.Contains("AL1", viewModel.SelectedInvoice.Patient);
        Assert.Contains("Payed", viewModel.StatusFilters);
        Assert.Equal(Brush.Parse("#D8F5D0").ToString(), viewModel.SelectedInvoice.StatusBackground.ToString());
        Assert.Equal(Brush.Parse("#D8F5D0").ToString(), viewModel.InvoiceStatusBackground.ToString());
        Assert.Equal("AL1 - Ada Lovelace", viewModel.PatientDetailsIdAndName);
        Assert.Equal("10.12.1985", viewModel.PatientDateOfBirth);
        Assert.Equal("GKV", viewModel.PatientInsuranceStatus);
        Assert.Equal("16.02.2026", viewModel.InvoiceDueDate);
        Assert.Single(viewModel.Positions);
        Assert.Equal("870", viewModel.Positions[0].BillingNumbers);
        Assert.True(viewModel.EditDraftCommand.CanExecute(null));
        Assert.False(viewModel.PrintInvoiceCommand.CanExecute(null));
    }

    [Fact]
    public void GivenIssuedInvoice_OverviewAllowsPrintingAgain()
    {
        // GIVEN
        var setup = CreateSetup();
        var invoice = setup.BillingService.ViewInvoices()[0];
        var issueResult = setup.BillingService.IssueInvoice(invoice, new DateTime(2026, 2, 2), setup.PracticeData);
        Assert.True(issueResult.Ok);

        var viewModel = new InvoicesViewModel(
            setup.BillingService,
            setup.PatientRepository,
            setup.Exporter,
            setup.Session,
            setup.NavigationService);

        try
        {
            // WHEN
            viewModel.PrintInvoiceCommand.Execute(null);

            // THEN
            Assert.False(viewModel.EditDraftCommand.CanExecute(null));
            Assert.True(viewModel.PrintInvoiceCommand.CanExecute(null));
            Assert.Equal(Brush.Parse("#FFE7A3").ToString(), viewModel.SelectedInvoice!.StatusBackground.ToString());
            Assert.Equal(Brush.Parse("#FFE7A3").ToString(), viewModel.InvoiceStatusBackground.ToString());
            Assert.NotNull(setup.Exporter.LastFilePath);
            Assert.EndsWith($"Invoice_{invoice.InvoiceNumber}.pdf", setup.Exporter.LastFilePath);
            Assert.IsType<InvoicePdfModel>(setup.Exporter.LastInvoice);
        }
        finally
        {
            if (Directory.Exists(setup.ExportDirectory))
            {
                Directory.Delete(setup.ExportDirectory, true);
            }
        }
    }

    private static TestSetup CreateSetup()
    {
        var invoiceRepository = new InMemoryInvoiceRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();

        var patient = new Patient("Ada", "Lovelace", "AL1");
        patient.SetDateOfBirth(new DateOnly(1985, 12, 10));
        patient.SetInsuranceStatus(PatientInsuranceStatus.GKV);
        patientRepository.Add(patient);

        var appointment = new Appointment(new DateTime(2026, 1, 12, 14, 0, 0), patient.ID);
        appointment.SetDuration(50);
        appointment.AssignBillingNumber(new BillingNumber("870", 1.00m, 120.00m, "Therapie", BillingNumberType.Privat));
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
            new DateTime(2026, 2, 2),
            14);
        Assert.True(addResult.Ok);

        var session = new ProjectSession();
        session.SetPracticeData(practiceData);
        var exportDirectory = Path.Combine(Path.GetTempPath(), "therapay-overview-test", Guid.NewGuid().ToString("N"));
        session.SetPracticeDataPath(Path.Combine(exportDirectory, "practice.csv"));

        return new TestSetup(
            billingService,
            patientRepository,
            new CapturingInvoicePdfExporter(),
            session,
            new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider()),
            practiceData,
            exportDirectory);
    }

    private sealed record TestSetup(
        BillingService BillingService,
        IPatientRepository PatientRepository,
        CapturingInvoicePdfExporter Exporter,
        ProjectSession Session,
        NavigationService NavigationService,
        PracticeData PracticeData,
        string ExportDirectory);

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
}
