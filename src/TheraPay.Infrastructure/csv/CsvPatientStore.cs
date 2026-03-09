using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvPatientStore
{
    private readonly string _filePath;

    public CsvPatientStore(string filePath)
    {
        _filePath = filePath;
    }

    public void SaveAll(IEnumerable<Patient> patients)
    { 
        var records = patients.Select(ToRecord).ToList();

        using var writer = new StreamWriter(_filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        csv.WriteRecords(records);
    }

    public List<Patient> LoadAll()
    {
        if (!File.Exists(_filePath))
            return new List<Patient>();

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<PatientCsvRecord>();

        return records.Select(ToDomain).ToList();
    }

    private static PatientCsvRecord ToRecord(Patient patient)
    {
        return new PatientCsvRecord
        {
            Id = patient.ID,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
        };
    }

    private static Patient ToDomain(PatientCsvRecord record)
    {
        return new Patient(record.FirstName, record.LastName, record.Id);
    }
}