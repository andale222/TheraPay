using TheraPay.Infrastructure.Export.Pdf;
using TheraPay.Core.Export;

namespace TheraPay.Infrastructure.Export.Tests;

public class MigraDocInvoicePdfExporter_test
{
    [Fact]
    public void Export_CreatesPdfFile_FromInvoicePdfModel()
    {
        EnsureRequiredFontsForExporter();

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
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m),
                new(
                    AppointmentStart: new DateTime(2026, 3, 8, 9, 0, 0),
                    Factor: 1.90m,
                    Description: "Psychotherapeutische Sprechstunde (25 Min.)",
                    AmountEuro: 70.00m)
            },
            TotalAmountEuro: 1370.03m);

        var outputDir = Path.Combine(AppContext.BaseDirectory, "Assets", "TheraPay.Infrastructure.Export.Tests");
        Directory.CreateDirectory(outputDir);
        var outputFile = Path.Combine(outputDir, "invoice.pdf");

        try
        {
            var exporter = new MigraDocInvoicePdfExporter();
            exporter.InternalExport(model, outputFile);

            Assert.True(File.Exists(outputFile));
            Assert.True(new FileInfo(outputFile).Length > 0);
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                // Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    private static void EnsureRequiredFontsForExporter()
    {
        var fontDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");
        Directory.CreateDirectory(fontDir);

        var regularTarget = Path.Combine(fontDir, "NotoSans-Regular.ttf");
        var boldTarget = Path.Combine(fontDir, "NotoSans-Bold.ttf");

        if (!File.Exists(regularTarget))
        {
            File.Copy(FindExistingFont(false), regularTarget, overwrite: true);
        }

        if (!File.Exists(boldTarget))
        {
            File.Copy(FindExistingFont(true), boldTarget, overwrite: true);
        }
    }

    private static string FindExistingFont(bool bold)
    {
        var candidates = bold
            ? new[]
            {
                "/usr/share/fonts/truetype/poppins/Poppins-Bold.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
                "/usr/share/fonts/truetype/liberation2/LiberationSans-Bold.ttf",
                @"C:\Windows\Fonts\arialbd.ttf",
                "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
            }
            : new[]
            {
                "/usr/share/fonts/truetype/poppins/Poppins-Regular.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf",
                @"C:\Windows\Fonts\arial.ttf",
                "/System/Library/Fonts/Supplemental/Arial.ttf"
            };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException(
            "Keine passende Systemschrift gefunden. Lege NotoSans-Regular.ttf und NotoSans-Bold.ttf im Test-Output unter Assets/Fonts ab.");
    }
}
