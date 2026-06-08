using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Core.Export;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;
using TheraPay.UI.ViewModels.Panels;

namespace TheraPay.UI.Tests;

public class InvoiceCreationViewModel_test
{
    [Fact]
    public void GivenSelectedAppointmentAlreadyInDraft_ContinueToDraft_WarnsAndRequiresConfirmation()
    {
        // GIVEN
        var invoiceRepository = new InMemoryInvoiceRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var patient = new Patient("Ada", "Lovelace", "AL1");
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(60);
        patientRepository.Add(patient);
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
        var existingDraftResult = billingService.AddInvoiceForPatientAndAppointments(
            patient.ID,
            [appointment.Id],
            practiceData,
            new DateTime(2026, 1, 5),
            14);
        Assert.True(existingDraftResult.Ok);

        var session = new ProjectSession();
        session.SetPracticeData(practiceData);
        var patientService = new PatientService(patientRepository);
        var appointmentService = new AppointmentService(appointmentRepository);
        NavigationService? navigationService = null;
        var services = new ServiceCollection();
        services.AddTransient(_ => new InvoiceDraftViewModel(
            billingService,
            patientRepository,
            new NoopInvoicePdfExporter(),
            session,
            navigationService!));
        var serviceProvider = services.BuildServiceProvider();
        navigationService = new NavigationService(new NavigationStore(), serviceProvider);
        var patientsPanel = new PatientPanelViewModel(patientService, navigationService);
        patientsPanel.SelectPatient(patient.ID);
        var viewModel = new InvoiceCreationViewModel(
            patientService,
            appointmentService,
            billingService,
            patientsPanel,
            patientRepository,
            session,
            navigationService);

        // WHEN
        viewModel.NavigateInvoiceDraftCommand.Execute(null);

        // THEN
        Assert.Single(billingService.ViewInvoices());
        Assert.Contains("bereits in anderen Drafts", viewModel.DraftStatusMessage);
        Assert.Equal("Trotzdem Draft erstellen", viewModel.DraftActionText);

        // WHEN
        viewModel.NavigateInvoiceDraftCommand.Execute(null);

        // THEN
        Assert.Equal(2, billingService.ViewInvoices().Count);
        Assert.Equal("Weiter zum Draft", viewModel.DraftActionText);
    }

    private sealed class NoopInvoicePdfExporter : IInvoicePdfExporter
    {
        public bool InternalExport(InvoicePdfModel invoice, string filePath)
        {
            return true;
        }
    }
}
