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
            Name = patient.FirstName + " " + patient.LastName,
            Id = patient.ID
        };
        var appointmentData = new List<InvoiceAppointmentData>();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

        //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.Name, invoice.PatientData.Name);
        Assert.Equal(patient.ID, invoice.PatientData.Id);
    }
    [Fact]
    public void GivenInvoicesAppointmentData_CreateInvoice_InvoiceHasCorrectAppointmentData()
    {
        // GIVEN
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Appointment appointment = new Appointment(date, "OR5");
        var patientData = new InvoicePatientData();
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment.Id.ToString("D"),
                Date = appointment.Date
            }
        };

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

        //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(appointment.Id.ToString("D"), invoice.AppointmentDataList[0].AppointmentId);
        Assert.Equal(date, invoice.AppointmentDataList[0].Date);
    }
    [Fact]
    public void GivenEmptyData_CreateInvoice_InvoiceIsDraft()
    {
        // GIVEN
        var patientData = new InvoicePatientData();
        var appointmentData = new List<InvoiceAppointmentData>();

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
        var appointmentData = new List<InvoiceAppointmentData>();
        var invoice = new Invoice(patientData, appointmentData);

        // WHEN
        invoice.Issue();

        // THEN
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }
}