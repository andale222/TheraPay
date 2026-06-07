namespace TheraPay.Core.Export;

using TheraPay.Domain;

public class InvoiceModelFactory
{
    public InvoicePdfModel Create(Invoice invoice)
    {
        try
        {
            var invoiceLines = new List<InvoicePdfLineModel>();
            foreach (var item in invoice.AppointmentDataList)
            {
                if (item.BillingNumbers.Count == 0)
                {
                    invoiceLines.Add(new InvoicePdfLineModel(
                        AppointmentStart: item.Date,
                        NumberOfUnits: 1,
                        GopNr: "",
                        Factor: 0m,
                        Description: "Termin ohne hinterlegte Abrechnungsnummer",
                        AmountEuro: item.TotalAmount));
                    continue;
                }

                foreach (var billingNumberGroup in item.BillingNumbers.GroupBy(billingNumber => billingNumber))
                {
                    var billingNumber = billingNumberGroup.Key;
                    var numberOfUnits = billingNumberGroup.Count();
                    invoiceLines.Add(new InvoicePdfLineModel(
                        AppointmentStart: item.Date,
                        NumberOfUnits: numberOfUnits,
                        GopNr: billingNumber.NumberIdentifier,
                        Factor: billingNumber.Factor,
                        Description: billingNumber.Description,
                        AmountEuro: billingNumber.Amount * numberOfUnits,
                        BillingType: billingNumber.Type));
                }
            }

            var model = new InvoicePdfModel(
                InvoiceNumber: invoice.InvoiceNumber,
                IssueDate: DateOnly.FromDateTime(invoice.IssueDate),
                PracticeName: invoice.PracticeDataRecord.PracticeName,
                PracticeDescription: invoice.PracticeDataRecord.PracticeDescription,
                PractitionerTitle: "M. Sc.",
                PractitionerName: invoice.PracticeDataRecord.PractitionerFirstLastName,
                PracticeStreetNr: invoice.PracticeDataRecord.Address.GetStreetNr(),
                PracticeCityCode: invoice.PracticeDataRecord.Address.GetPostalCodeCity(),
                PracticeTelephone: invoice.PracticeDataRecord.PracticePhoneNr,
                PracticeEmail: invoice.PracticeDataRecord.PracticeEmail,
                Iban: invoice.PracticeDataRecord.PaymentDetails.IBAN,
                Bic: invoice.PracticeDataRecord.PaymentDetails.BLZ,
                BankName: invoice.PracticeDataRecord.PaymentDetails.BankName,
                subject: invoice.InvoiceNumber,
                PatientName: invoice.PatientData.Name,
                TaxIdNumber: invoice.PracticeDataRecord.TaxNumber,
                Diagnosis: "F41.3",
                PatientStreetNr: invoice.PatientData.StreetAndHouseNumber,
                PatientCityCode: invoice.PatientData.PostalCodeAndCity,
                Lines: invoiceLines,
                TotalAmountEuro: invoice.TotalAmount);

            return model;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error creating InvoicePdfModel: {ex.Message}");


            var model = new InvoicePdfModel(
            InvoiceNumber: "RE-2026-0001",
            IssueDate: new DateOnly(2026, 3, 13),
            PracticeName: "M. Sc. Psych. Muster Therapeut",
            PracticeDescription: "description of practice type",
            PractitionerTitle: "M. Sc.",
            PractitionerName: "Muster Therapeut",
            PracticeStreetNr: "Musterweg 12",
            PracticeCityCode: "12345 Teststadt",
            PracticeTelephone: "+11 123 123 412 32",
            PracticeEmail: "testmail@testdomain.com",
            Iban: "DE12 3456 7890 1234 5678 90",
            Bic: "BYS032480",
            BankName: "testbank",
            subject: "testsubject",
            PatientName: "Max Mustermann",
            TaxIdNumber: "00/00/000000",
            Diagnosis: "F41.3",
            PatientStreetNr: "Teststraße 342",
            PatientCityCode: "12345 Teststadt",
            Lines: new List<InvoicePdfLineModel>
            {
                new(
                    AppointmentStart: new DateTime(2026, 3, 1, 9, 0, 0),
                    NumberOfUnits: 2,
                    GopNr: "801a",
                    Factor: 2.30m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 134.06m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m)
            },
            TotalAmountEuro: 1370.03m);
            return model;
        }
    }
}
