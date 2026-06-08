using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace TheraPay.Infrastructure.csv;

public abstract class CsvStore<TDomain, TRecord> where TDomain : class where TRecord : class
{
    private readonly string _filePath;

    public CsvStore(string filePath)
    {
        _filePath = filePath;
    }

    public void SaveAll(IEnumerable<TDomain> data)
    { 
        var records = data.Select(ToRecord).ToList();

        using var writer = new StreamWriter(_filePath);
        using var csv = new CsvWriter(writer, CreateCsvConfiguration());
        
        csv.WriteRecords(records);
    }

    public List<TDomain> LoadAll()
    {
        if (!File.Exists(_filePath))
            return new List<TDomain>();

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CreateCsvConfiguration());

        var records = csv.GetRecords<TRecord>();

        return records.Select(ToDomain).ToList();
    }

    protected abstract TRecord ToRecord(TDomain objectToConvert);

    protected abstract TDomain ToDomain(TRecord record);

    protected virtual CsvConfiguration CreateCsvConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture);
    }
}
