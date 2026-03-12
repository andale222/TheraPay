using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvAppointmentStore : CsvStore<Appointment, AppointmentCsvRecord>
{

    public CsvAppointmentStore(string filePath) : base(filePath)
    {
    }


    protected override AppointmentCsvRecord ToRecord(Appointment objectToConvert)
    {
        return new AppointmentCsvRecord
        {
            StartDateTime = objectToConvert.Date.ToString("o", CultureInfo.InvariantCulture),
            Duration = objectToConvert.DurationInMinutes.ToString(),
            PatientId = objectToConvert.PatientID,
            IsDeleted = false
        };
    }

    protected override Appointment ToDomain(AppointmentCsvRecord record)
    {
        var appointment = new Appointment(DateTime.Parse(record.StartDateTime), record.PatientId);
        appointment.SetDuration(int.Parse(record.Duration));

        return appointment;
    }
}