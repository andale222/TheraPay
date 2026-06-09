using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv;

public sealed class InvoiceCsvRecord
{
    public string Id { get; set; } = "";
    public InvoiceStatus Status { get; set; }
    public string IssueDate { get; set; } = "";
    public string DueDate { get; set; } = "";
    public string InvoiceNumber { get; set; } = "";
    public string TotalAmount { get; set; } = "";
    public string AdditionalText { get; set; } = "";
    public string Subject { get; set; } = "";
    public string PatientDataJson { get; set; } = "";
    public string PracticeDataJson { get; set; } = "";
    public string AppointmentDataListJson { get; set; } = "";
}
