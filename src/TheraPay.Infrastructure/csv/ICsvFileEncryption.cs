namespace TheraPay.Infrastructure.csv;

public interface ICsvFileEncryption
{
    byte[] ReadPlaintext(string filePath);

    void WritePlaintext(string filePath, byte[] plaintext);

    void EncryptFile(string plaintextFilePath, string encryptedFilePath);

    void DecryptFile(string encryptedFilePath, string plaintextFilePath);
}
