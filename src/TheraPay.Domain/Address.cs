using System.Text.RegularExpressions;

namespace TheraPay.Domain;

public sealed record Address
{
    public const string PostalCodePattern = @"^\d{5}$";
    public static readonly Regex PostalCodeRegex = new(PostalCodePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Street { get; }
    public string HouseNumber { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string? Country { get; }
    public string? Additional { get; }

    public Address(
        string street,
        string houseNumber,
        string postalCode,
        string city,
        string? country = null,
        string? additional = null)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required.", nameof(street));
            if (string.IsNullOrWhiteSpace(houseNumber))
                throw new ArgumentException("House number is required.", nameof(houseNumber));
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Postal code is required.", nameof(postalCode));
            if (PostalCodeRegex.IsMatch(postalCode.Trim()) == false)
                throw new ArgumentException("Postal code must be exactly 5 digits.", nameof(postalCode));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required.", nameof(city));

        Street = street.Trim();
        HouseNumber = houseNumber.Trim();
        PostalCode = postalCode.Trim();
        City = city.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        Additional = string.IsNullOrWhiteSpace(additional) ? null : additional.Trim();
    }

    public string GetStreetNr()
    {
        return $"{Street} {HouseNumber}";
    }

    public string GetPostalCodeCity()
    {
        return $"{PostalCode} {City}";
    }
}
