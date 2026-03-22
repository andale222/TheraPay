using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;


public class InMemoryInvoiceRepository_test
{
    [Fact]
    public void Given_CreateInMemoryInvoiceRepository_test_RepositoryIsEmpty()
    {
        // GIVEN

        // WHEN
        InMemoryInvoiceRepository repository = new InMemoryInvoiceRepository();

        // THEN
        Assert.NotNull(repository);
        Assert.Equal(0, repository.Count());
    }

    [Fact]
    public void GivenEmptyInMemoryInvoiceRepository_AddInvoice_CountIsOne()
    {
        // GIVEN
        InMemoryInvoiceRepository repository = new InMemoryInvoiceRepository();
        Invoice invoice = TestData.CreateInvoice();

        // WHEN
        repository.Add(invoice);

        // THEN
        Assert.Equal(1, repository.Count());
        Assert.Equal(invoice, repository.GetByIndex(0));
    }

    [Fact]
    public void GivenEmptyInMemoryInvoiceRepository_AddTwoInvoices_CountIsTwo()
    {
        // GIVEN
        InMemoryInvoiceRepository repository = new InMemoryInvoiceRepository();
        Invoice invoice1 = TestData.CreateInvoice();
        Invoice invoice2 = TestData.CreateInvoice();

        // WHEN
        repository.Add(invoice1);
        repository.Add(invoice2);

        // THEN
        Assert.Equal(2, repository.Count());
        Assert.Equal(invoice1, repository.GetByIndex(0));
        Assert.Equal(invoice2, repository.GetByIndex(1));
    }

    [Fact]
    public void GivenInMemoryInvoiceRepository_GetAll_ReturnsAllInvoices()
    {
        // GIVEN
        InMemoryInvoiceRepository repository = new InMemoryInvoiceRepository();
        Invoice invoice1 = TestData.CreateInvoice();
        Invoice invoice2 = TestData.CreateInvoice();

        // WHEN
        repository.Add(invoice1);
        repository.Add(invoice2);
        var allAppointments = repository.GetAll();

        // THEN
        Assert.Equal(2, allAppointments.Count());
    }

    
    [Fact]
    public void GivenEmptyInMemoryInvoiceRepository_AddInvoiceTwice_CountIsOneReturnsBadResult()
    {
        // GIVEN
        InMemoryInvoiceRepository repository = new InMemoryInvoiceRepository();
        Invoice invoice1 = TestData.CreateInvoice();

        // WHEN
        repository.Add(invoice1);
        var result = repository.Add(invoice1);

        // THEN
        Assert.False(result.Ok);
        Assert.Equal($"Invoice with ID {invoice1.Id} already exists.", result.Error);
        Assert.Equal(1, repository.Count());
    }

}