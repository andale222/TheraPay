namespace TheraPay.Infrastructure.Export.Pdf;

public sealed record InvoicePdfModel(
    string InvoiceNumber,
    DateOnly IssueDate,
    string PracticeName,
    string PracticeDescription,
    string PractitionerName,
    string PractitionerTitle,
    string PracticeStreetNr,
    string PracticeCityCode,
    string PracticeTelephone,
    string PracticeEmail,
    string Iban,
    string Bic,
    string BankName,
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