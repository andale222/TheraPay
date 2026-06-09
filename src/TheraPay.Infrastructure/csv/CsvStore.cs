using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TheraPay.Infrastructure.csv;

public abstract class CsvStore<TDomain, TRecord> where TDomain : class where TRecord : class
{
    private readonly string _filePath;
    private readonly ICsvFileEncryption _fileEncryption;

    public CsvStore(string filePath, ICsvFileEncryption? fileEncryption = null)
    {
        _filePath = filePath;
        _fileEncryption = fileEncryption ?? MockCsvFileEncryption.Instance;
    }

    public void SaveAll(IEnumerable<TDomain> data)
    { 
        var records = data.Select(ToRecord).ToList();

        using var buffer = new MemoryStream();
        using (var writer = new StreamWriter(buffer, new UTF8Encoding(false), leaveOpen: true))
        using (var csv = new CsvWriter(writer, CreateCsvConfiguration()))
        {
            csv.WriteRecords(records);
        }

        byte[] plaintext = buffer.ToArray();
        try
        {
            _fileEncryption.WritePlaintext(_filePath, plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public List<TDomain> LoadAll()
    {
        if (!File.Exists(_filePath))
            return new List<TDomain>();

        byte[] plaintext = _fileEncryption.ReadPlaintext(_filePath);
        using var buffer = new MemoryStream(plaintext, writable: false);
        using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
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
