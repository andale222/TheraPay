using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvPatientStore(string filePath) : CsvStore<Patient, PatientCsvRecord>(filePath)
{
    protected override PatientCsvRecord ToRecord(Patient patient)
    {
        return new PatientCsvRecord
        {
            Id = patient.ID,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
        };
    }

    protected override Patient ToDomain(PatientCsvRecord record)
    {
        return new Patient(record.FirstName, record.LastName, record.Id);
    }
}