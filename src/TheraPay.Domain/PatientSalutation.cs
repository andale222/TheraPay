namespace TheraPay.Domain;

public static class PatientSalutation
{
    public static IReadOnlyList<string> Options { get; } =
    [
        "Herr",
        "Frau",
        "Divers"
    ];

    public static bool TryNormalize(string salutation, out string normalizedSalutation)
    {
        normalizedSalutation = "";
        string value = salutation.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return true;

        string? option = Options.FirstOrDefault(candidate =>
            string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase));
        if (option is null)
            return false;

        normalizedSalutation = option;
        return true;
    }
}
