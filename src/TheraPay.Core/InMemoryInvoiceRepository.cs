namespace TheraPay.Core;

using TheraPay.Domain;

public class InMemoryInvoiceRepository : InMemoryRepositoryBase<Invoice>, IInvoiceRepository
{
    protected override Result EntityExists(Invoice entity)
    {
        bool exists = Items.Any(p => p.Id == entity.Id);
        if (exists)
            return new Result(exists, $"Invoice with ID {entity.Id} already exists.");

        return new Result(false, $"Invoice with ID {entity.Id} not found.");
    }
}