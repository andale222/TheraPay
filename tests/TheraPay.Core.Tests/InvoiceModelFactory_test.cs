using TheraPay.Core.Export;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;

public class InvoiceModelFactory_test
{
    [Fact]
    public void GivenInvoiceWithPatientAddress_Create_UsesPatientAddressInPdfModel()
    {
        // GIVEN
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patient.SetAddress("Imaginary Road", "42B", "12345", "London", "UK", "near the analytical engine");
        var patientData = InvoicePatientData.FromPatientData(patient);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(60);
        var billingNumber = BillingNumberCatalog.FindByIdentifier("801a")!;
        appointment.AssignBillingNumber(billingNumber);
        var appointmentData = new List<InvoiceAppointmentData>
        {
            InvoiceAppointmentData.FromAppointmentData(appointment)
        };
        var practiceData = TestData.PracticeData1();
        var invoice = new Invoice(patientData, appointmentData, PracticeDataRecord.FromPracticeData(practiceData));
        invoice.Issue(PracticeDataRecord.FromPracticeData(practiceData), $"{DateTime.Today:yyyyMM}-1201");
        var factory = new InvoiceModelFactory();

        // WHEN
        var model = factory.Create(invoice);

        // THEN
        Assert.Equal("Imaginary Road 42B", model.PatientStreetNr);
        Assert.Equal("12345 London", model.PatientCityCode);
    }

    [Fact]
    public void GivenInvoiceWithBillingNumbers_Create_UsesBillingNumbersInPdfLines()
    {
        // GIVEN
        var patient = TestData.Patient1();
        var patientData = InvoicePatientData.FromPatientData(patient);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        var billingNumber = BillingNumberCatalog.FindByIdentifier("801a")!;
        appointment.AssignBillingNumber(billingNumber);
        appointment.AssignBillingNumber(billingNumber);
        var appointmentData = new List<InvoiceAppointmentData>
        {
            InvoiceAppointmentData.FromAppointmentData(appointment)
        };
        var practiceData = TestData.PracticeData1();
        var invoice = new Invoice(patientData, appointmentData, PracticeDataRecord.FromPracticeData(practiceData));
        invoice.Issue(PracticeDataRecord.FromPracticeData(practiceData), $"{DateTime.Today:yyyyMM}-1201");
        var factory = new InvoiceModelFactory();

        // WHEN
        var model = factory.Create(invoice);

        // THEN
        Assert.Single(model.Lines);
        Assert.Equal(2, model.Lines[0].NumberOfUnits);
        Assert.Equal(billingNumber.NumberIdentifier, model.Lines[0].GopNr);
        Assert.Equal(billingNumber.Factor, model.Lines[0].Factor);
        Assert.Equal(billingNumber.Description, model.Lines[0].Description);
        Assert.Equal(billingNumber.Amount * 2, model.Lines[0].AmountEuro);
        Assert.Equal(billingNumber.Type, model.Lines[0].BillingType);
    }

    [Fact]
    public void GivenInvoiceWithPaymentTerm_Create_UsesPaymentTermInPdfModel()
    {
        // GIVEN
        var patient = TestData.Patient1();
        var patientData = InvoicePatientData.FromPatientData(patient);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        var appointmentData = new List<InvoiceAppointmentData>
        {
            InvoiceAppointmentData.FromAppointmentData(appointment)
        };
        var practiceData = TestData.PracticeData1();
        practiceData.DefaultPaymentTermDays = 21;
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);
        var issueDate = new DateTime(2026, 3, 13);
        invoice.Issue(practiceDataRecord, $"{issueDate:yyyyMM}-1201", issueDate);
        var factory = new InvoiceModelFactory();

        // WHEN
        var model = factory.Create(invoice);

        // THEN
        Assert.Equal(issueDate, model.IssueDate.ToDateTime(TimeOnly.MinValue));
        Assert.Equal(21, model.PaymentTermInDays);
    }
}
