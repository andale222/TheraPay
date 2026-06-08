using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheraPay.Infrastructure.csv;

public class CsvAppointmentStore(string filePath) : CsvStore<Appointment, AppointmentCsvRecord>(filePath)
{
    private static readonly JsonSerializerOptions BillingNumberJsonOptions = new()
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

    protected override AppointmentCsvRecord ToRecord(Appointment objectToConvert)
    {
        return new AppointmentCsvRecord
        {
            Id = objectToConvert.Id.ToString("D"),
            StartDateTime = objectToConvert.Date.ToString("o", CultureInfo.InvariantCulture),
            Duration = objectToConvert.DurationInMinutes.ToString(),
            PatientId = objectToConvert.PatientID,
            BillingNumbersJson = SerializeBillingNumbers(objectToConvert.BillingNumbers),
            IsDeleted = false,
            Status = objectToConvert.Status
        };
    }

    protected override Appointment ToDomain(AppointmentCsvRecord record)
    {
        var date = DateTime.Parse(record.StartDateTime, CultureInfo.InvariantCulture);
        var duration = int.Parse(record.Duration);
        var id = Guid.Parse(record.Id);
        var billingNumbers = DeserializeBillingNumbers(record.BillingNumbersJson);
        return Appointment.Rehydrate(id, date, record.PatientId, duration, record.Status, billingNumbers);
    }

    private static string SerializeBillingNumbers(IReadOnlyList<BillingNumber> billingNumbers)
    {
        return JsonSerializer.Serialize(billingNumbers, BillingNumberJsonOptions);
    }

    private static List<BillingNumber> DeserializeBillingNumbers(string billingNumbersJson)
    {
        if (string.IsNullOrWhiteSpace(billingNumbersJson))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<BillingNumber>>(billingNumbersJson, BillingNumberJsonOptions) ?? [];
    }
}
