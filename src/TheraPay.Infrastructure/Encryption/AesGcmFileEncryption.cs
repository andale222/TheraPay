using System.Security.Cryptography;
using System.Text;

namespace TheraPay.Infrastructure.Encryption;

public sealed class AesGcmFileEncryption : IFileEncryption
{
    private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("TPCSVGCM");
    private const byte Version = 1;
    private const int MaxHeaderFieldLength = 1024;
    private const int MinTagSizeInBytes = 12;
    private const int MaxTagSizeInBytes = 16;

    private readonly string _password;
    private readonly AesGcmFileEncryptionOptions _options;

    public AesGcmFileEncryption(string password, AesGcmFileEncryptionOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("A password is required for AES-GCM file encryption.", nameof(password));

        _password = password;
        _options = options ?? new AesGcmFileEncryptionOptions();
        ValidateOptions(_options);
    }

    public byte[] ReadPlaintext(string filePath)
    {
        using var input = new MemoryStream(File.ReadAllBytes(filePath), writable: false);
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: false);

        var magic = reader.ReadBytes(MagicBytes.Length);
        if (!magic.SequenceEqual(MagicBytes))
            throw new InvalidDataException("The file is not a TheraPay AES-GCM encrypted file.");

        byte version = reader.ReadByte();
        if (version != Version)
            throw new InvalidDataException($"Unsupported encrypted file version: {version}.");

        int iterations = reader.ReadInt32();
        if (iterations <= 0)
            throw new InvalidDataException("Encrypted file contains an invalid PBKDF2 iteration count.");

        byte[] salt = ReadLengthPrefixedBytes(reader, "salt");
        byte[] nonce = ReadLengthPrefixedBytes(reader, "nonce");
        byte[] tag = ReadLengthPrefixedBytes(reader, "tag");
        byte[] cipherText = reader.ReadBytes(checked((int)(input.Length - input.Position)));

        byte[] key = DeriveKey(salt, iterations);
        byte[] plaintext = new byte[cipherText.Length];

        try
        {
            using var aes = new AesGcm(key, tag.Length);
            aes.Decrypt(nonce, cipherText, tag, plaintext);
            return plaintext;
        }
        catch
        {
            CryptographicOperations.ZeroMemory(plaintext);
            throw;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public void WritePlaintext(string filePath, byte[] plaintext)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(_options.SaltSizeInBytes);
        byte[] nonce = RandomNumberGenerator.GetBytes(_options.NonceSizeInBytes);
        byte[] tag = new byte[_options.TagSizeInBytes];
        byte[] cipherText = new byte[plaintext.Length];
        byte[] key = DeriveKey(salt, _options.Pbkdf2Iterations);

        try
        {
            using var aes = new AesGcm(key, _options.TagSizeInBytes);
            aes.Encrypt(nonce, plaintext, cipherText, tag);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }

        using var output = new MemoryStream();
        using (var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(MagicBytes);
            writer.Write(Version);
            writer.Write(_options.Pbkdf2Iterations);
            WriteLengthPrefixedBytes(writer, salt);
            WriteLengthPrefixedBytes(writer, nonce);
            WriteLengthPrefixedBytes(writer, tag);
            writer.Write(cipherText);
        }

        File.WriteAllBytes(filePath, output.ToArray());
    }

    public void EncryptFile(string plaintextFilePath, string encryptedFilePath)
    {
        byte[] plaintext = File.ReadAllBytes(plaintextFilePath);
        try
        {
            WritePlaintext(encryptedFilePath, plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public void DecryptFile(string encryptedFilePath, string plaintextFilePath)
    {
        byte[] plaintext = ReadPlaintext(encryptedFilePath);
        try
        {
            File.WriteAllBytes(plaintextFilePath, plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static byte[] ReadLengthPrefixedBytes(BinaryReader reader, string fieldName)
    {
        int length = reader.ReadInt32();
        if (length is <= 0 or > MaxHeaderFieldLength)
            throw new InvalidDataException($"Encrypted file contains an invalid {fieldName} length.");

        byte[] bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException($"Encrypted file ended while reading {fieldName}.");

        return bytes;
    }

    private static void WriteLengthPrefixedBytes(BinaryWriter writer, byte[] bytes)
    {
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private byte[] DeriveKey(byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            _password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            _options.KeySizeInBytes);
    }

    private static void ValidateOptions(AesGcmFileEncryptionOptions options)
    {
        if (options.Pbkdf2Iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "PBKDF2 iterations must be greater than zero.");

        if (options.SaltSizeInBytes < 8)
            throw new ArgumentOutOfRangeException(nameof(options), "Salt size must be at least 8 bytes.");

        if (options.NonceSizeInBytes != 12)
            throw new ArgumentOutOfRangeException(nameof(options), "AES-GCM nonce size must be 12 bytes.");

        if (options.TagSizeInBytes is < MinTagSizeInBytes or > MaxTagSizeInBytes)
            throw new ArgumentOutOfRangeException(nameof(options), "AES-GCM tag size must be between 12 and 16 bytes.");

        if (options.KeySizeInBytes is not (16 or 24 or 32))
            throw new ArgumentOutOfRangeException(nameof(options), "AES key size must be 16, 24, or 32 bytes.");
    }
}
