using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;

public class BillingService_test
{

    [Fact]
    public void GivenInvoiceRepository_CreateBillingService_BillingServiceHasRepository()
    {
        // GIVEN
        InMemoryInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        InMemoryAppointmentRepository appointmentRepo = new InMemoryAppointmentRepository();
        InMemoryPatientRepository patientRepo = new InMemoryPatientRepository();

        // WHEN
        BillingService service = new BillingService(invoiceRepo, appointmentRepo, patientRepo);

        // THEN
        Assert.Equal(invoiceRepo.GetAll(), service.ViewInvoices());
    }
    [Fact]
    public void GivenAppointmentsAndPatients_AddInvoiceForPatientAndAppointmentsWithCorrectData_InvoiceIsAddedToInvoiceRepository()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);
        // Patient1 has two appointments, one is already billed. Patient2 has one appointment.

        // WHEN
        var patientId = TestData.Patient1().ID;
        var aptmtId = appointmentRepo.GetByIndex(0).Id;
        List<Guid> appointmentIds = [aptmtId];
        var result = service.AddInvoiceForPatientAndAppointments(patientId, appointmentIds, TestData.PracticeData1());

        // THEN
        Assert.True(result.Ok);
        var invoices = service.ViewInvoices();
        Assert.Single(invoices);
        Assert.Equal(patientId, invoices[0].PatientData.Id);
        Assert.Single(invoices[0].AppointmentDataList);
        Assert.Equal(aptmtId.ToString("D"), invoices[0].AppointmentDataList[0].AppointmentId);
    }

    [Fact]
    public void GivenPatientWithAddress_AddInvoiceForPatientAndAppointments_InvoiceContainsPatientAddress()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = new InMemoryAppointmentRepository();
        IPatientRepository patientRepo = new InMemoryPatientRepository();
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patient.SetAddress("Imaginary Road", "42B", "12345", "London", "UK", "near the analytical engine");
        patientRepo.Add(patient);

        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(60);
        appointmentRepo.Add(appointment);

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);

        // WHEN
        var result = service.AddInvoiceForPatientAndAppointments(
            patient.ID,
            [appointment.Id],
            TestData.PracticeData1());

        // THEN
        Assert.True(result.Ok);
        var invoice = Assert.Single(service.ViewInvoices());
        Assert.Equal("Imaginary Road", invoice.PatientData.Street);
        Assert.Equal("42B", invoice.PatientData.HouseNumber);
        Assert.Equal("12345", invoice.PatientData.PostalCode);
        Assert.Equal("London", invoice.PatientData.City);
        Assert.Equal("UK", invoice.PatientData.Country);
        Assert.Equal("near the analytical engine", invoice.PatientData.AddressAdditional);
    }

    [Fact]
    public void GivenInvoiceDraftDetails_AddInvoiceForPatientAndAppointments_InvoiceContainsIssueDateAndPaymentTerm()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        var service = new BillingService(invoiceRepo, appointmentRepo, patientRepo);
        var patientId = TestData.Patient1().ID;
        var appointmentId = appointmentRepo.GetByIndex(0).Id;
        var issueDate = new DateTime(2026, 3, 13);

        // WHEN
        var result = service.AddInvoiceForPatientAndAppointments(
            patientId,
            [appointmentId],
            TestData.PracticeData1(),
            issueDate,
            21,
            "Zusatztext");

        // THEN
        Assert.True(result.Ok);
        var invoice = Assert.Single(service.ViewInvoices());
        Assert.Equal(issueDate, invoice.IssueDate);
        Assert.Equal(issueDate.AddDays(21), invoice.DueDate);
        Assert.Equal(21, invoice.PracticeDataRecord.DefaultPaymentTermDays);
        Assert.Equal("Zusatztext", invoice.AdditionalText);
    }

    [Fact]
    public void GivenDraftAndIssueDate_IssueInvoice_UsesIssueDateAndPaymentTerm()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        var service = new BillingService(invoiceRepo, appointmentRepo, patientRepo);
        var practiceData = TestData.PracticeData1();
        practiceData.DefaultPaymentTermDays = 21;
        var patientId = TestData.Patient1().ID;
        var appointmentId = appointmentRepo.GetByIndex(0).Id;
        var issueDate = new DateTime(2026, 3, 13);
        service.AddInvoiceForPatientAndAppointments(patientId, [appointmentId], practiceData, issueDate, 21);
        var invoice = service.ViewInvoices()[0];

        // WHEN
        var result = service.IssueInvoice(invoice, issueDate, practiceData);

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(issueDate, invoice.IssueDate);
        Assert.Equal(issueDate.AddDays(21), invoice.DueDate);
        Assert.StartsWith("202603-", invoice.InvoiceNumber);
    }

    [Fact]
    public void GivenAppointmentsAndPatients_AddInvoiceForPatientAndAppointmentsWithMismatchingPatientId_InvoiceIsNotAddedToInvoiceRepository()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);
        // Patient1 has two appointments, one is already billed. Patient2 has one appointment.

        // WHEN
        var patientId = TestData.Patient2().ID;
        var aptmtId = appointmentRepo.GetByIndex(0).Id;
        List<Guid> appointmentIds = [aptmtId];
        var result = service.AddInvoiceForPatientAndAppointments(patientId, appointmentIds, TestData.PracticeData1());

        // THEN
        Assert.False(result.Ok);
        Assert.Empty(service.ViewInvoices());
        // check that the error is caught in the service!
        Assert.Equal("Mismatch between patient Id and appointments patient Id.",result.Error);
    }
    [Fact]
    public void GivenAppointmentsAndPatients_AddInvoiceForPatientAndAppointmentsWithMismatchingPatientIdInAppointments_InvoiceIsNotAddedToInvoiceRepository()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();

        var appointment = TestData.Appointment2();
        appointment.SetDuration(30);
        appointmentRepo.Add(appointment);

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);
        // Patient1 has two appointments, one is already billed. Patient2 has one appointment.

        // WHEN
        var patientId = TestData.Patient1().ID;
        var aptmtId = appointmentRepo.GetByIndex(0).Id;
        var aptmtId2 = appointment.Id;
        List<Guid> appointmentIds = [aptmtId,aptmtId2];
        var result = service.AddInvoiceForPatientAndAppointments(patientId, appointmentIds, TestData.PracticeData1());

        // THEN
        Assert.False(result.Ok);
        Assert.Empty(service.ViewInvoices());
        // check that the error is caught in the service!
        Assert.Equal("Mismatch between patient Id and appointments patient Id.",result.Error);
    }
    [Fact]
    public void GivenAppointmentsAndPatients_AddInvoiceForPatientAndAppointmentsWithOneBilledAppointment_InvoiceIsNotAddedToInvoiceRepository()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();

        var appointment1_2 = TestData.Appointment1_2();
        appointment1_2.SetStatusToBilled();
        appointmentRepo.Add(appointment1_2);

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);
        // Patient1 has two appointments, one is already billed. Patient2 has one appointment.

        // WHEN
        var patientId = TestData.Patient1().ID;
        var aptmtId = appointmentRepo.GetByIndex(0).Id;
        var aptmtId2 = appointment1_2.Id;
        List<Guid> appointmentIds = [aptmtId,aptmtId2];
        var result = service.AddInvoiceForPatientAndAppointments(patientId, appointmentIds, TestData.PracticeData1());

        // THEN
        Assert.True(result.Ok);
        Assert.Single(service.ViewInvoices()[0].AppointmentDataList);
        // check that the error is caught in the service!
        Assert.Equal("1 billed appointments were removed. ",result.Error);
    }

    [Fact]
    public void GivenAppointmentsAndPatients_AddInvoiceForPatientAndTwoAppointmentsOfSameId_InvoiceIsAddedToInvoiceRepositoryAndTheDoubleEntryIgnored()
    {
        // GIVEN
        IInvoiceRepository invoiceRepo = new InMemoryInvoiceRepository();
        IAppointmentRepository appointmentRepo = TestData.getInMemoryInMemoryAppointmentRepositoryWithTwoAppointments();
        IPatientRepository patientRepo = TestData.getInMemoryPatientRepositoryWithTwoPatients();

        BillingService service = new(invoiceRepo, appointmentRepo, patientRepo);
        // Patient1 has two appointments, one is already billed. Patient2 has one appointment.

        // WHEN
        var patientId = TestData.Patient1().ID;
        var aptmtId = appointmentRepo.GetByIndex(0).Id;
        List<Guid> appointmentIds = [aptmtId,aptmtId];
        var result = service.AddInvoiceForPatientAndAppointments(patientId, appointmentIds, TestData.PracticeData1());

        // THEN
        Assert.True(result.Ok);
        Assert.Equal("1 double appointment entry was removed.", result.Error);
        Assert.Single(service.ViewInvoices()[0].AppointmentDataList);
    }
}
