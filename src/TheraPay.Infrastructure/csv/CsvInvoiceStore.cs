using CsvHelper.Configuration;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv;

public class CsvInvoiceStore : CsvStore<Invoice, InvoiceCsvRecord>
{
    public CsvInvoiceStore(string filePath, ICsvFileEncryption? fileEncryption = null)
        : base(filePath, fileEncryption)
    {
    }

    private static readonly JsonSerializerOptions InvoiceJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected override CsvConfiguration CreateCsvConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };
    }

    protected override InvoiceCsvRecord ToRecord(Invoice invoice)
    {
        return new InvoiceCsvRecord
        {
            Id = invoice.Id.ToString("D"),
            Status = invoice.Status,
            IssueDate = FormatDate(invoice.IssueDate),
            DueDate = FormatDate(invoice.DueDate),
            InvoiceNumber = invoice.InvoiceNumber,
            TotalAmount = invoice.TotalAmount.ToString(CultureInfo.InvariantCulture),
            AdditionalText = invoice.AdditionalText,
            Subject = invoice.Subject,
            PatientDataJson = Serialize(invoice.PatientData),
            PracticeDataJson = Serialize(invoice.PracticeDataRecord),
            AppointmentDataListJson = Serialize(invoice.AppointmentDataList)
        };
    }

    protected override Invoice ToDomain(InvoiceCsvRecord record)
    {
        var id = Guid.Parse(record.Id);
        var patientData = DeserializeRequired<InvoicePatientData>(record.PatientDataJson, nameof(record.PatientDataJson));
        var practiceData = DeserializeRequired<PracticeDataRecord>(record.PracticeDataJson, nameof(record.PracticeDataJson));
        var appointmentData = DeserializeRequired<List<InvoiceAppointmentData>>(
            record.AppointmentDataListJson,
            nameof(record.AppointmentDataListJson));

        return Invoice.Rehydrate(
            id,
            patientData,
            appointmentData,
            practiceData,
            record.Status,
            ParseDate(record.IssueDate),
            ParseDate(record.DueDate),
            record.InvoiceNumber,
            record.AdditionalText,
            record.Subject);
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, InvoiceJsonOptions);
    }

    private static T DeserializeRequired<T>(string json, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException($"{fieldName} is required.");

        return JsonSerializer.Deserialize<T>(json, InvoiceJsonOptions)
            ?? throw new InvalidDataException($"{fieldName} could not be deserialized.");
    }

    private static string FormatDate(DateTime date)
    {
        return date == default ? "" : date.ToString("o", CultureInfo.InvariantCulture);
    }

    private static DateTime ParseDate(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return default;

        return DateTime.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}
