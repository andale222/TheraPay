
using TheraPay.Domain;
using TheraPay.Core.Export;

namespace TheraPay.Core;

public interface IInvoicePdfExporter
{
    bool Export(Invoice invoice, string filePath, bool includePaymentQrCode = true)
    {
        var modelFactory = new InvoiceModelFactory();
        var model = modelFactory.Create(invoice) with { IncludePaymentQrCode = includePaymentQrCode };
        return InternalExport(model, filePath);
    }

    protected abstract bool InternalExport(InvoicePdfModel invoice, string filePath);
}
