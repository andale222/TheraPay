namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    public InvoiceAppointmentData AppointmentData { get; private set; }
    public Guid Id { get; }
    public InvoiceStatus Status { get; private set; }

    public Invoice(InvoicePatientData patientData, InvoiceAppointmentData appointmentData)
    {
        Id = Guid.NewGuid();
        PatientData = patientData;
        AppointmentData = appointmentData;
        Status = InvoiceStatus.Draft;
    }

    private bool IsEditable( ) => Status == InvoiceStatus.Draft;

    public void Issue()
    {
        if (!IsEditable())
            return;

        Status = InvoiceStatus.Issued;
    }
}


public enum InvoiceStatus { Draft, Issued, Cancelled };

public sealed record InvoicePatientData
{
    public string PatientName { get; set; } = "";
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; set;}
    public string PatientId { get; set; } = "";
}