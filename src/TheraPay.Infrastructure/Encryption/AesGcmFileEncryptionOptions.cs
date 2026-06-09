namespace TheraPay.Infrastructure.Encryption;

public sealed class AesGcmFileEncryptionOptions
{
    public int Pbkdf2Iterations { get; init; } = 210_000;

    public int SaltSizeInBytes { get; init; } = 16;

    public int NonceSizeInBytes { get; init; } = 12;

    public int TagSizeInBytes { get; init; } = 16;

    public int KeySizeInBytes { get; init; } = 32;
}
