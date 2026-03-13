namespace TheraPay.Infrastructure.Export.Pdf;

public sealed record InvoicePdfModel(
    string InvoiceNumber,
    DateOnly IssueDate,
    string PracticeName,
    string PractitionerName,
    string PractitionerTitle,
    string PracticeStreetNr,
    string PracticeCityCode,
    string subject,
    string TaxIdNumber,
    string PatientName,
    string Diagnosis,
    string PatientStreetNr,
    string PatientCityCode,
    IReadOnlyList<InvoicePdfLineModel> Lines,
    decimal TotalAmountEuro);

public sealed record InvoicePdfLineModel(
    DateTime AppointmentStart,
    decimal Factor,
    string Description,
    decimal AmountEuro);