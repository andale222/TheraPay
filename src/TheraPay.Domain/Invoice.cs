namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    private List<InvoiceAppointmentData> _appointmentDataList = new List<InvoiceAppointmentData>();
    public IReadOnlyList<InvoiceAppointmentData> AppointmentDataList => _appointmentDataList;
    public Guid Id { get; }
    public InvoiceStatus Status { get; private set; }

    public Invoice(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList)
    {
        Id = Guid.NewGuid();
        if (!CheckDataValidity(patientData, appointmentDataList))
        {
            throw new Exception("Irgendwas ist schiefgelaufen.");
        }
        PatientData = patientData;
        _appointmentDataList = appointmentDataList;
        Status = InvoiceStatus.Draft;
    }

    private bool CheckDataValidity(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList)
    {
        foreach (var appointment in appointmentDataList)
            if (patientData.Id != appointment.PatientId)
                return false;

        return true;
    }

    private bool IsEditable() => Status == InvoiceStatus.Draft;

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
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; set; }
    public string AppointmentId { get; set; } = "";
    public string PatientId { get; set; } = "";
}