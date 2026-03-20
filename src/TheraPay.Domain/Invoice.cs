namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    public PracticeDataRecord PracticeDataRecord { get; private set; }
    private List<InvoiceAppointmentData> _appointmentDataList = new List<InvoiceAppointmentData>();
    public IReadOnlyList<InvoiceAppointmentData> AppointmentDataList => _appointmentDataList;
    public Guid Id { get; }
    public InvoiceStatus Status { get; private set; }
    public decimal TotalAmount {get; private set; }
    public DateTime IssueDate {get;private set;}
    public DateTime DueDate {get;private set;}

    public Invoice(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList, PracticeData practiceData)
    {
        Id = Guid.NewGuid();
        if (patientData == null)
            throw new ArgumentNullException(nameof(patientData));
        if (appointmentDataList == null)
            throw new ArgumentNullException(nameof(appointmentDataList));
        if (practiceData == null)
            throw new ArgumentNullException(nameof(practiceData));
        if (!CheckDataValidity(patientData, appointmentDataList))
        {
            throw new ArgumentException("Data inconsistency detected: multiple patient Ids or matching appointment Ids detected.");
        }
        PatientData = patientData;
        PracticeDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
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

    private void UpdateTotalAmount()
    {
        // TODO: update the total amount computation as soon as it is implemented in the appointments...
    }

    private bool IsEditable() => Status == InvoiceStatus.Draft;

    public void Issue()
    {
        if (!IsEditable())
            return;

        IssueDate = DateTime.Today;
        DueDate = IssueDate.AddDays(PracticeDataRecord.DefaultPaymentTermDays);
        Status = InvoiceStatus.Issued;
    }
}


public enum InvoiceStatus { Draft, Issued, Overdue, Cancelled };

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

public sealed record PracticeDataRecord
{
    public string Name { get; init; } = "";
    public Address Address { get; init; } = new Address("street", "1", "00000", "city");
    public string TaxNumber { get; init; } = "";
    public PaymentDetails PaymentDetails { get; init; } = new PaymentDetails("DE00");
    public int DefaultPaymentTermDays { get; init; }

    public static PracticeDataRecord FromPracticeData(PracticeData practiceData)
    {
        if (practiceData == null)
            throw new ArgumentNullException(nameof(practiceData));

        return new PracticeDataRecord
        {
            Name = practiceData.Name,
            Address = new Address(
                practiceData.Street,
                practiceData.HouseNumber,
                practiceData.PostalCode,
                practiceData.City,
                practiceData.Country,
                practiceData.AddressAdditional),
            TaxNumber = practiceData.TaxIdentificationNumber,
            PaymentDetails = new PaymentDetails(
                practiceData.IBAN,
                practiceData.BLZ,
                practiceData.BankName,
                practiceData.Subject),
            DefaultPaymentTermDays = practiceData.DefaultPaymentTermDays
        };
    }
}
