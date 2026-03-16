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
        if (patientData == null)
            throw new ArgumentNullException(nameof(patientData));
        if (appointmentDataList == null)
            throw new ArgumentNullException(nameof(appointmentDataList));
        if (!CheckDataValidity(patientData, appointmentDataList))
        {
            throw new ArgumentException("Data inconsistency detected: multiple patient Ids or matching appointment Ids detected.");
        }
        PatientData = patientData;
        _appointmentDataList = appointmentDataList.ToList();
        Status = InvoiceStatus.Draft;
    }

    private bool CheckDataValidity(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList)
    {
        foreach (var appointment in appointmentDataList)
            if (patientData.Id != appointment.PatientId)
                return false;

        var ids = appointmentDataList.Select(x => x.AppointmentId).ToList();
        if (ids.Count != ids.Distinct().Count())
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
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; init; }
    public string AppointmentId { get; init; } = "";
    public string PatientId { get; init; } = "";
}