namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    public Guid Id { get; }

    public Invoice( InvoicePatientData patientData )
    {
        Id = Guid.NewGuid();
        PatientData = patientData;
    }
}

public sealed record InvoicePatientData
{
    public string PatientName { get; set; } = "";
}