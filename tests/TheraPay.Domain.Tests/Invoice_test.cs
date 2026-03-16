namespace TheraPay.Domain.Tests;

public class Invoice_test
{
    // public static Appointment CreateAppointment() => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
        Patient patient = new Patient("A", "J", "L5R");
        var patientData = new InvoicePatientData()
        {
            PatientName = patient.FirstName+" "+patient.LastName
        };
        var appointmentData = new InvoiceAppointmentData();

    //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

    //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.PatientName, invoice.PatientData.PatientName);
    }
    [Fact]
    public void GivenInvoicesAppointmentData_CreateInvoice_InvoiceHasCorrectAppointmentData()
    {
        // GIVEN
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        string patientID = "OR5";
        Appointment appointment = new Appointment( date, patientID);
        var patientData = new InvoicePatientData();
        var appointmentData = new InvoiceAppointmentData()
        {
            PatientId = appointment.PatientID,
            Date = appointment.Date
        };

    //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

    //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.PatientName, invoice.PatientData.PatientName);
        Assert.Equal(patientID, invoice.AppointmentData.PatientId);
        Assert.Equal(date, invoice.AppointmentData.Date);
    }
    [Fact]
    public void GivenEmptyData_CreateInvoice_InvoiceIsDraft()
    {
        // GIVEN
        var patientData = new InvoicePatientData();
        var appointmentData = new InvoiceAppointmentData();

        // WHEN
        var invoice = new Invoice(patientData, appointmentData);

        // THEN
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
    [Fact]
    public void GivenEmptyInvoice_IssueInvoice_StatusIsIssued()
    {
        // GIVEN
        var patientData = new InvoicePatientData();
        var appointmentData = new InvoiceAppointmentData();
        var invoice = new Invoice(patientData, appointmentData);

        // WHEN
        invoice.Issue();

        // THEN
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }
}