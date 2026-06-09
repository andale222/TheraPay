namespace TheraPay.Infrastructure.Encryption;

public sealed class DummyFileEncryption : IFileEncryption
{
    public static DummyFileEncryption Instance { get; } = new();

    private DummyFileEncryption()
    {
    }

    public byte[] ReadPlaintext(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    public void WritePlaintext(string filePath, byte[] plaintext)
    {
        File.WriteAllBytes(filePath, plaintext);
    }

    public void EncryptFile(string plaintextFilePath, string encryptedFilePath)
    {
        WritePlaintext(encryptedFilePath, ReadPlaintext(plaintextFilePath));
    }

    public void DecryptFile(string encryptedFilePath, string plaintextFilePath)
    {
        WritePlaintext(plaintextFilePath, ReadPlaintext(encryptedFilePath));
    }
}
