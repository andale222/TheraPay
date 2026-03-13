using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public class CsvPracticeDataStore : IPracticeDataStore
{
    private readonly string _filePath;

    public CsvPracticeDataStore(string filePath)
    {
        _filePath = filePath;
    }

    public void Save(PracticeData data)
    { 
        var record = ToRecord(data);

        using var writer = new StreamWriter(_filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        csv.WriteRecords(new[] { record });
    }

    public PracticeData Load()
    {
        if (!File.Exists(_filePath))
            return new PracticeData();

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<CsvPracticeDataRecord>();

        return ToDomain(records.FirstOrDefault());
    }

    private static CsvPracticeDataRecord ToRecord(PracticeData data)
    {
        return new CsvPracticeDataRecord
        {
            Name = data.Name,
            FirstNamePractitioner = data.FirstNamePractitioner,
            LastNamePractitioner = data.LastNamePractitioner,
            StreetAndNumber = data.StreetAndNumber,
            CityAndPostalCode = data.CityAndPostalCode,
            Country = data.Country,
            PhoneNumber = data.PhoneNumber,
            IBAN = data.IBAN,
            TaxIdentificationNumber = data.TaxIdentificationNumber,
        };
    }
    private static PracticeData ToDomain(CsvPracticeDataRecord record)
    {
        return new PracticeData
        {
            Name = record.Name,
            FirstNamePractitioner = record.FirstNamePractitioner,
            LastNamePractitioner = record.LastNamePractitioner,
            StreetAndNumber = record.StreetAndNumber,
            CityAndPostalCode = record.CityAndPostalCode,
            Country = record.Country,
            PhoneNumber = record.PhoneNumber,
            IBAN = record.IBAN,
            TaxIdentificationNumber = record.TaxIdentificationNumber,
        };
    }
}