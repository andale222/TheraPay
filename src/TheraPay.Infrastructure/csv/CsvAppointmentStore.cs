using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvAppointmentStore(string filePath) : CsvStore<Appointment, AppointmentCsvRecord>(filePath)
{
    protected override AppointmentCsvRecord ToRecord(Appointment objectToConvert)
    {
        return new AppointmentCsvRecord
        {
            Id = objectToConvert.Id.ToString("D"),
            StartDateTime = objectToConvert.Date.ToString("o", CultureInfo.InvariantCulture),
            Duration = objectToConvert.DurationInMinutes.ToString(),
            PatientId = objectToConvert.PatientID,
            IsDeleted = false
        };
    }

    protected override Appointment ToDomain(AppointmentCsvRecord record)
    {
        var date = DateTime.Parse(record.StartDateTime, CultureInfo.InvariantCulture);
        var duration = int.Parse(record.Duration);
        var id = Guid.Parse(record.Id);
        return Appointment.Rehydrate(id, date, record.PatientId, duration);
    }
}
