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
}
