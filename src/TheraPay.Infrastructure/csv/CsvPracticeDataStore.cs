using TheraPay.Core;
using TheraPay.Domain;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TheraPay.Infrastructure.Encryption;

namespace TheraPay.Infrastructure.csv;

public class CsvPracticeDataStore : IPracticeDataStore
{
    private readonly string _filePath;
    private readonly IFileEncryption _fileEncryption;

    public CsvPracticeDataStore(string filePath, IFileEncryption? fileEncryption = null)
    {
        _filePath = filePath;
        _fileEncryption = fileEncryption ?? DummyFileEncryption.Instance;
    }

    public void Save(PracticeData data)
    { 
        var record = ToRecord(data);

        using var buffer = new MemoryStream();
        using (var writer = new StreamWriter(buffer, new UTF8Encoding(false), leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(new[] { record });
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

    public PracticeData Load()
    {
        if (!File.Exists(_filePath))
            return new PracticeData();

        byte[] plaintext = _fileEncryption.ReadPlaintext(_filePath);
        using var buffer = new MemoryStream(plaintext, writable: false);
        using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<CsvPracticeDataRecord>();


        return ToDomain(records.FirstOrDefault() ?? new CsvPracticeDataRecord());
    }

    private static CsvPracticeDataRecord ToRecord(PracticeData data)
    {
        return new CsvPracticeDataRecord
        {
            Name = data.Name,
            PracticeDescription = data.PracticeDescription,
            FirstNamePractitioner = data.FirstNamePractitioner,
            LastNamePractitioner = data.LastNamePractitioner,
            Street = data.Street,
            HouseNumber = data.HouseNumber,
            PostalCode = data.PostalCode,
            City = data.City,
            Country = data.Country,
            AddressAdditional = data.AddressAdditional,
            PhoneNumber = data.PhoneNumber,
            PracticeEmail = data.PracticeEmail,
            IBAN = data.IBAN,
            BLZ = data.BLZ,
            BankName = data.BankName,
            Subject = data.Subject,
            TaxIdentificationNumber = data.TaxIdentificationNumber,
            DefaultPaymentTermDays = data.DefaultPaymentTermDays,
            InvoiceStateYear = data.InvoiceNumberState.Year,
            InvoiceStateRandomStart = data.InvoiceNumberState.RandomStart,
            InvoiceStateNextIssueNumber = data.InvoiceNumberState.NextIssueNumber,
        };
    }
    private static PracticeData ToDomain(CsvPracticeDataRecord record)
    {
        return new PracticeData
        {
            Name = record.Name,
            PracticeDescription = record.PracticeDescription,
            FirstNamePractitioner = record.FirstNamePractitioner,
            LastNamePractitioner = record.LastNamePractitioner,
            Street = record.Street,
            HouseNumber = record.HouseNumber,
            PostalCode = record.PostalCode,
            City = record.City,
            Country = record.Country,
            AddressAdditional = record.AddressAdditional,
            PhoneNumber = record.PhoneNumber,
            PracticeEmail = record.PracticeEmail,
            IBAN = record.IBAN,
            BLZ = record.BLZ,
            BankName = record.BankName,
            Subject = record.Subject,
            TaxIdentificationNumber = record.TaxIdentificationNumber,
            DefaultPaymentTermDays = record.DefaultPaymentTermDays,
            InvoiceNumberState = InvoiceNumberState.Rehydrate(
                record.InvoiceStateYear,
                record.InvoiceStateRandomStart,
                record.InvoiceStateNextIssueNumber),
        };
    }
}
