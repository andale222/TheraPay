namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    public InvoiceAppointmentData AppointmentData { get; private set; }
    public Guid Id { get; }

    public Invoice(InvoicePatientData patientData, InvoiceAppointmentData appointmentData)
    {
        Id = Guid.NewGuid();
        PatientData = patientData;
        AppointmentData = appointmentData;
    }
}

public sealed record InvoicePatientData
{
    public string PatientName { get; set; } = "";
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; set;}
    public string PatientId { get; set; } = "";
}