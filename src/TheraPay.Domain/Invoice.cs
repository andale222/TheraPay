

namespace TheraPay.Domain;

public class Invoice
{
    public InvoicePatientData PatientData { get; private set; }
    public PracticeDataRecord PracticeDataRecord { get; private set; }
    private List<InvoiceAppointmentData> _appointmentDataList = new List<InvoiceAppointmentData>();
    public IReadOnlyList<InvoiceAppointmentData> AppointmentDataList => _appointmentDataList;
    public Guid Id { get; }
    public InvoiceStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string InvoiceNumber { get; private set; } = "";
    public string AdditionalText { get; private set; } = "";

    public Invoice(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList, PracticeDataRecord practiceDataRecord)
    {
        Id = Guid.NewGuid();
        if (patientData == null)
            throw new ArgumentNullException(nameof(patientData));
        if (appointmentDataList == null)
            throw new ArgumentNullException(nameof(appointmentDataList));
        if (practiceDataRecord == null)
            throw new ArgumentNullException(nameof(practiceDataRecord));
        if (!CheckDataValidity(patientData, appointmentDataList))
        {
            throw new ArgumentException("Data inconsistency detected: multiple patient Ids or matching appointment Ids detected.");
        }
        PatientData = patientData;
        PracticeDataRecord = practiceDataRecord;
        _appointmentDataList = appointmentDataList.ToList();
        Status = InvoiceStatus.Draft;
        UpdateTotalAmount();
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
        TotalAmount = 0m;
        foreach (var appointment in AppointmentDataList)
        {
            TotalAmount += appointment.TotalAmount;
        }
    }

    private bool IsEditable() => Status == InvoiceStatus.Draft;

    private static bool InvoiceNumberFormatIsOk(string invoiceNumber, DateTime issueDate)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber) || invoiceNumber.Length != 11)
            return false;

        if (invoiceNumber[6] != '-')
            return false;

        if ($"{issueDate:yyyyMM}" != invoiceNumber.Substring(0, 6))
            return false;


        for (var i = 7; i < invoiceNumber.Length; i++)
        {
            if (!char.IsDigit(invoiceNumber[i]))
                return false;
        }

        return true;
    }
    public Result Issue(PracticeDataRecord practiceData, string invoiceNumber)
    {
        if (!IsEditable())
            return new Result(false, "Issue is not editable anymore.");

        var draftIssueDate = DateTime.Today;
        if (!InvoiceNumberFormatIsOk(invoiceNumber, draftIssueDate))
            return new Result(false, "Error in invoice number or invoice number format.");

        if (practiceData == null)
            return new Result(false, "Given practice Data Record is empty.");

        IssueDate = draftIssueDate;
        PracticeDataRecord = practiceData;
        UpdateTotalAmount();
        InvoiceNumber = invoiceNumber;
        DueDate = IssueDate.AddDays(PracticeDataRecord.DefaultPaymentTermDays);
        Status = InvoiceStatus.Issued;

        return new Result(true);
    }

    // private string GenerateInvoiceNumber(DateTime issueDate)
    // {
    //     // Locking the state object ensures unique numbers in one process.
    //     lock (_practiceData.InvoiceNumberState)
    //     {
    //         var serial = _practiceData.InvoiceNumberState.ConsumeNextSerial(issueDate);
    //         return $"{issueDate:yyyyMM}-{serial:0000}";
    //     }
    // }
}


public enum InvoiceStatus { Draft, Issued, Overdue, Cancelled };

public sealed record InvoicePatientData
{
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
    public static InvoicePatientData FromPatientData(Patient data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return new InvoicePatientData
        {
            Name = data.FirstName + " " + data.LastName,
            Id = data.ID
        };
    }
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; init; }
    public string AppointmentId { get; init; } = "";
    public string PatientId { get; init; } = "";
    public decimal TotalAmount { get; init; } = 0m;
    public static InvoiceAppointmentData FromAppointmentData(Appointment data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return new InvoiceAppointmentData
        {
            AppointmentId = data.Id.ToString("D"),
            Date = data.Date,
            PatientId = data.PatientID,
            TotalAmount = data.TotalAmount
        };
    }
}
