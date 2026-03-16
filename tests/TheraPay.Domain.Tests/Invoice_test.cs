namespace TheraPay.Domain.Tests;

public class Invoice_test
{
    // public static Appointment CreateAppointment() => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "patientID");
    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
    //     DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Patient patient = new Patient("A", "J", "L5R");
        var patientData = new InvoicePatientData()
        {
            PatientName = patient.FirstName+" "+patient.LastName
        };

    //     // WHEN
        var invoice = new Invoice(patientData);

    //     // THEN
    //     Assert.Equal(patient.ID, appointment.PatientID);
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.PatientName, invoice.PatientData.PatientName);
    }
    
}