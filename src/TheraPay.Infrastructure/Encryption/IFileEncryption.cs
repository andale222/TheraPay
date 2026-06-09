namespace TheraPay.Infrastructure.Encryption;

public interface IFileEncryption
{
    byte[] ReadPlaintext(string filePath);

    void WritePlaintext(string filePath, byte[] plaintext);

    void EncryptFile(string plaintextFilePath, string encryptedFilePath);

    void DecryptFile(string encryptedFilePath, string plaintextFilePath);
}
