namespace TheraPay.Domain;

public sealed record Address
{
    public string Street { get; init; }
    public string HouseNumber { get; init; }
    public string PostalCode { get; init; }
    public string City { get; init; }
    public string? Country { get; init; }
    public string? Additional { get; init; }

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
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        Street = street;
        HouseNumber = houseNumber;
        PostalCode = postalCode;
        City = city;
        Country = country;
        Additional = additional;
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
