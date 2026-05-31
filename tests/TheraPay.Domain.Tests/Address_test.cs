using TheraPay.Domain;

namespace TheraPay.Domain.Tests;

public class Address_test
{
    [Fact]
    public void GivenAddress_GetStreetNr_ReturnsStreetAndHouseNumber()
    {
        // GIVEN
        var address = new Address("Teststraße", "81A", "12345", "Teststadt", "Deutschland", "2. OG");

        // WHEN
        var streetNr = address.GetStreetNr();

        // THEN
        Assert.Equal("Teststraße 81A", streetNr);
    }

    [Fact]
    public void GivenAddress_GetPostalCodeCity_ReturnsPostalCodeAndCity()
    {
        // GIVEN
        var address = new Address("Teststraße", "81A", "12345", "Teststadt", "Deutschland", "2. OG");

        // WHEN
        var postalCodeCity = address.GetPostalCodeCity();

        // THEN
        Assert.Equal("12345 Teststadt", postalCodeCity);
    }

    [Fact]
    public void GivenAddressWithoutOptionalFields_CreateAddress_OptionalFieldsAreNull()
    {
        // WHEN
        var address = new Address("Teststraße", "81A", "12345", "Teststadt");

        // THEN
        Assert.Null(address.Country);
        Assert.Null(address.Additional);
    }

    [Fact]
    public void GivenInvalidPostalCode_CreateAddress_ThrowsArgumentException()
    {
        // THEN
        Assert.Throws<ArgumentException>(() => new Address("Teststraße", "81A", "1234", "Teststadt"));
    }
}
