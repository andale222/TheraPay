using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvInvoiceStore_test
{
    [Fact]
    public void GivenInvoices_SaveAllAndLoadAll_RoundTripsDraftIssuedPayedAndCancelledInvoices()
    {
        // GIVEN
        var filePath = TestPaths.DataFile("testSaveInvoices.csv");
        var store = new CsvInvoiceStore(filePath);
        var draft = CreateDraftInvoice("P1", new DateTime(2026, 1, 1), "801a");
        var issued = CreateDraftInvoice("P2", new DateTime(2026, 1, 2), "812a");
        var payed = CreateDraftInvoice("P3", new DateTime(2026, 1, 3), "860");
        var cancelled = CreateDraftInvoice("P4", new DateTime(2026, 1, 4), "861");

        var issueDate = new DateTime(2026, 2, 1);
        issued.Issue(issued.PracticeDataRecord, $"{issueDate:yyyyMM}-1201", issueDate);
        payed.Issue(payed.PracticeDataRecord, $"{issueDate:yyyyMM}-1202", issueDate);
        payed.SetPostIssueStatus(InvoiceStatus.Payed, issueDate);
        cancelled.Issue(cancelled.PracticeDataRecord, $"{issueDate:yyyyMM}-1203", issueDate);
        cancelled.SetPostIssueStatus(InvoiceStatus.Cancelled, issueDate);

        try
        {
            // WHEN
            store.SaveAll([draft, issued, payed, cancelled]);
            var loaded = store.LoadAll();

            // THEN
            Assert.Equal(4, loaded.Count);
            AssertInvoiceRoundTrip(draft, loaded.Single(invoice => invoice.Id == draft.Id));
            AssertInvoiceRoundTrip(issued, loaded.Single(invoice => invoice.Id == issued.Id));
            AssertInvoiceRoundTrip(payed, loaded.Single(invoice => invoice.Id == payed.Id));
            AssertInvoiceRoundTrip(cancelled, loaded.Single(invoice => invoice.Id == cancelled.Id));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    private static Invoice CreateDraftInvoice(string patientId, DateTime appointmentDate, string billingNumberIdentifier)
    {
        var patient = new Patient("Test", patientId, patientId);
        patient.SetAddress("Testweg", "1", "12345", "Teststadt");
        var appointment = new Appointment(appointmentDate, patient.ID);
        appointment.AssignBillingNumber(BillingNumberCatalog.FindByIdentifier(billingNumberIdentifier)!);

        var practiceData = new PracticeData
        {
            Name = "Praxis Test",
            Street = "Praxisweg",
            HouseNumber = "2",
            PostalCode = "54321",
            City = "Praxisstadt",
            TaxIdentificationNumber = "123",
            IBAN = "DE00",
            DefaultPaymentTermDays = 14
        };

        var invoice = new Invoice(
            InvoicePatientData.FromPatientData(patient),
            [InvoiceAppointmentData.FromAppointmentData(appointment)],
            PracticeDataRecord.FromPracticeData(practiceData));
        invoice.SetDraftDetails(new DateTime(2026, 2, 1), 14, "Hinweis", "Therapie");
        return invoice;
    }

    private static void AssertInvoiceRoundTrip(Invoice expected, Invoice actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.IssueDate, actual.IssueDate);
        Assert.Equal(expected.DueDate, actual.DueDate);
        Assert.Equal(expected.InvoiceNumber, actual.InvoiceNumber);
        Assert.Equal(expected.TotalAmount, actual.TotalAmount);
        Assert.Equal(expected.AdditionalText, actual.AdditionalText);
        Assert.Equal(expected.Subject, actual.Subject);
        Assert.Equal(expected.PatientData.Id, actual.PatientData.Id);
        Assert.Equal(expected.PatientData.Name, actual.PatientData.Name);
        Assert.Equal(expected.PracticeDataRecord.PracticeName, actual.PracticeDataRecord.PracticeName);
        Assert.Single(actual.AppointmentDataList);
        Assert.Equal(expected.AppointmentDataList[0].AppointmentId, actual.AppointmentDataList[0].AppointmentId);
        Assert.Equal(expected.AppointmentDataList[0].Date, actual.AppointmentDataList[0].Date);
        Assert.Equal(expected.AppointmentDataList[0].TotalAmount, actual.AppointmentDataList[0].TotalAmount);
    }
}
