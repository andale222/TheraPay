namespace TheraPay.Domain.Tests;

public class Invoice_test
{
    // public static Appointment CreateAppointment() => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Patient patient = new Patient("A", "J", "L5R");
        Appointment appointment = new Appointment( date, patient.ID);
        var patientData = new InvoicePatientData()
        {
            PatientName = patient.FirstName+" "+patient.LastName
        };
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
        Assert.Equal(appointment.PatientID, invoice.AppointmentData.PatientId);
        Assert.Equal(appointment.Date, invoice.AppointmentData.Date);
    }
    
}