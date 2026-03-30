namespace TheraPay.Core.Export;

using TheraPay.Domain;
public class InvoiceModelFactory
{
    public InvoicePdfModel Create(Invoice invoice)
    {
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