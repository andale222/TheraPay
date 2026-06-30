

namespace TheraPay.Domain;

public class Invoice
{
    public const string DefaultSubject = "Ambulante Psychotherapie";
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
    public string Subject { get; private set; } = DefaultSubject;

    public Invoice(InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList, PracticeDataRecord practiceDataRecord)
        : this(Guid.NewGuid(), patientData, appointmentDataList, practiceDataRecord)
    {
    }

    private Invoice(Guid id, InvoicePatientData patientData, List<InvoiceAppointmentData> appointmentDataList, PracticeDataRecord practiceDataRecord)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));
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
        Id = id;
        PatientData = patientData;
        PracticeDataRecord = practiceDataRecord;
        _appointmentDataList = appointmentDataList.ToList();
        Status = InvoiceStatus.Draft;
        UpdateTotalAmount();
    }

    public static Invoice Rehydrate(
        Guid id,
        InvoicePatientData patientData,
        List<InvoiceAppointmentData> appointmentDataList,
        PracticeDataRecord practiceDataRecord,
        InvoiceStatus status,
        DateTime issueDate,
        DateTime dueDate,
        string invoiceNumber,
        string additionalText,
        string subject)
    {
        var invoice = new Invoice(id, patientData, appointmentDataList, practiceDataRecord)
        {
            Status = status,
            IssueDate = issueDate,
            DueDate = dueDate,
            InvoiceNumber = invoiceNumber ?? "",
            AdditionalText = additionalText ?? "",
            Subject = string.IsNullOrWhiteSpace(subject) ? DefaultSubject : subject.Trim()
        };
        invoice.UpdateTotalAmount();
        return invoice;
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

    public Result SetDraftDetails(
        DateTime issueDate,
        int paymentTermInDays,
        string additionalText = "",
        string subject = DefaultSubject)
    {
        if (!IsEditable())
            return new Result(false, "Issue is not editable anymore.");

        if (paymentTermInDays < 0)
            return new Result(false, "Payment term cannot be negative.");

        IssueDate = issueDate.Date;
        PracticeDataRecord = PracticeDataRecord with { DefaultPaymentTermDays = paymentTermInDays };
        DueDate = IssueDate.AddDays(paymentTermInDays);
        AdditionalText = additionalText ?? "";
        Subject = string.IsNullOrWhiteSpace(subject) ? DefaultSubject : subject.Trim();

        return new Result(true);
    }

    public Result SetPatientData(InvoicePatientData patientData)
    {
        if (!IsEditable())
            return new Result(false, "Issue is not editable anymore.");

        if (patientData == null)
            return new Result(false, "Patient data cannot be null.");

        if (patientData.Id != PatientData.Id)
            return new Result(false, "Patient ID cannot be changed.");

        PatientData = patientData;
        return new Result(true);
    }

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
        return Issue(practiceData, invoiceNumber, DateTime.Today);
    }

    public Result Issue(PracticeDataRecord practiceData, string invoiceNumber, DateTime issueDate)
    {
        if (!IsEditable())
            return new Result(false, "Issue is not editable anymore.");

        var draftIssueDate = issueDate.Date;
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

    public Result RefreshOverdueStatus(DateTime referenceDate)
    {
        if (Status == InvoiceStatus.Issued && IsOverdue(referenceDate))
        {
            Status = InvoiceStatus.Overdue;
        }

        return new Result(true);
    }

    public Result SetPostIssueStatus(InvoiceStatus requestedStatus, DateTime referenceDate)
    {
        if (Status == InvoiceStatus.Draft)
            return new Result(false, "Draft invoices cannot be marked as issued, payed or cancelled.");

        if (requestedStatus is not (InvoiceStatus.Issued or InvoiceStatus.Payed or InvoiceStatus.Cancelled))
            return new Result(false, "Invoice status can only be set to Issued, Payed or Cancelled.");

        Status = requestedStatus == InvoiceStatus.Issued && IsOverdue(referenceDate)
            ? InvoiceStatus.Overdue
            : requestedStatus;

        return new Result(true);
    }

    private bool IsOverdue(DateTime referenceDate)
    {
        return DueDate != default && DueDate.Date < referenceDate.Date;
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


public enum InvoiceStatus { Draft, Issued, Overdue, Payed, Cancelled };

public sealed record InvoicePatientData
{
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
    public string Street { get; init; } = "";
    public string HouseNumber { get; init; } = "";
    public string PostalCode { get; init; } = "";
    public string City { get; init; } = "";
    public string Country { get; init; } = "";
    public string AddressAdditional { get; init; } = "";
    public string StreetAndHouseNumber => string.IsNullOrWhiteSpace(Street) && string.IsNullOrWhiteSpace(HouseNumber)
        ? ""
        : $"{Street} {HouseNumber}".Trim();
    public string PostalCodeAndCity => string.IsNullOrWhiteSpace(PostalCode) && string.IsNullOrWhiteSpace(City)
        ? ""
        : $"{PostalCode} {City}".Trim();
    public string Salutation { get; init; } = "";
    public string ICD10Diagnosis { get; init; } = "";

    public static InvoicePatientData FromPatientData(Patient data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return new InvoicePatientData
        {
            Name = data.FirstName + " " + data.LastName,
            Id = data.ID,
            Street = data.Address?.Street ?? "",
            HouseNumber = data.Address?.HouseNumber ?? "",
            PostalCode = data.Address?.PostalCode ?? "",
            City = data.Address?.City ?? "",
            Country = data.Address?.Country ?? "",
            AddressAdditional = data.Address?.Additional ?? "",
            Salutation = data.Salutation,
            ICD10Diagnosis = data.ICD10Diagnosis
        };
    }
}
public sealed record InvoiceAppointmentData
{
    public DateTime Date { get; init; }
    public string AppointmentId { get; init; } = "";
    public string PatientId { get; init; } = "";
    public decimal TotalAmount { get; init; } = 0m;
    public IReadOnlyList<BillingNumber> BillingNumbers { get; init; } = [];

    public static InvoiceAppointmentData FromAppointmentData(Appointment data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return new InvoiceAppointmentData
        {
            AppointmentId = data.Id.ToString("D"),
            Date = data.Date,
            PatientId = data.PatientID,
            TotalAmount = data.TotalAmount,
            BillingNumbers = data.BillingNumbers.ToList()
        };
    }
}
