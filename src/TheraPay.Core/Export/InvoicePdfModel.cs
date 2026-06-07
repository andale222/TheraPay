namespace TheraPay.Core.Export;

using TheraPay.Domain;

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
    string Subject,
    string TaxIdNumber,
    string PatientName,
    string Diagnosis,
    string PatientStreetNr,
    string PatientCityCode,
    IReadOnlyList<InvoicePdfLineModel> Lines,
    decimal TotalAmountEuro,
    int PaymentTermInDays = 14,
    string Anrede = "Sehr geehrte Frau ",
    bool IncludePaymentQrCode = true);

public sealed record InvoicePdfLineModel(
    DateTime AppointmentStart,
    decimal Factor,
    string Description,
    decimal AmountEuro,
    int NumberOfUnits = 1,
    string GopNr = "",
    BillingNumberType BillingType = BillingNumberType.Privat);
