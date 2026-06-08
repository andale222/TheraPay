using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using QRCoder;
using TheraPay.Core;
using TheraPay.Core.Export;

namespace TheraPay.Infrastructure.Export.Pdf;

public sealed class MigraDocInvoicePdfExporter : IInvoicePdfExporter
{
    private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");
    private const double PaymentQrCodeSizeCentimeters = 4;
    private const int PaymentQrCodeQuietZoneModules = 0;

    public bool InternalExport(InvoicePdfModel model, string filePath)
    {
        PdfFontBootstrap.Configure();

        var document = new Document();
        document.Info.Title = $"Rechnung {model.InvoiceNumber}";
        document.Info.Author = model.PracticeName;

        DefineStyles(document);

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.3);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2.0);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2.2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(2.2);

        BuildHeader(section, model);
        BuildPatientBlock(section, model);
        BuildInvoiceDetailsBlock(section, model);
        BuildLinesTable(section, model);
        BuildTotal(section, model);
        BuildFooter(section, model);
        // section.AddPageBreak();

        var renderer = new PdfDocumentRenderer();
        renderer.Document = document;
        renderer.RenderDocument();
        renderer.Save(filePath);

        return true;
    }

    private static void DefineStyles(Document document)
    {
        var normal = document.Styles["Normal"]!;
        normal.Font.Name = "TheraSans";
        normal.Font.Size = 11;

        // var footer = document.Styles["FooterSize"];
        // footer.Font.Name = "TheraSans";
        // footer.Font.Size = 8;

        var heading = document.Styles["Heading1"]!;
        heading.Font.Name = "TheraSans";
        heading.Font.Size = 13;
        heading.Font.Bold = true;
        heading.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.5);

        var style = document.Styles[StyleNames.Footer]!;
        style.ParagraphFormat.ClearAll();
        style.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(0), TabAlignment.Left);
        style.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(7), TabAlignment.Left);
        style.ParagraphFormat.TabStops.AddTabStop(Unit.FromCentimeter(16), TabAlignment.Right);
        style.ParagraphFormat.Font.Size = 8;

    }

    private static void BuildHeader(Section section, InvoicePdfModel model)
    {
        var title = section.AddParagraph($"{model.PracticeName}", "Heading1");
        title.Format.Alignment = ParagraphAlignment.Center;
        title.Format.SpaceAfter = Unit.FromCentimeter(0.1);

        var secondPar = section.AddParagraph($"{model.PracticeDescription}");
        secondPar.Format.Alignment = ParagraphAlignment.Center;
        secondPar.Format.Font.Bold = true;
        secondPar.Format.Font.Size = 10;

        var thirdPar = section.AddParagraph($"Tel.: {model.PracticeTelephone} · E-Mail: {model.PracticeEmail}");
        thirdPar.Format.Alignment = ParagraphAlignment.Center;
        thirdPar.Format.Font.Size = 10;


        HeaderFooter header = section.Headers.Primary;
        double[] foldPositionsMm = { 105, 210 };

        foreach (double posMm in foldPositionsMm)
        {
            TextFrame tf = header.AddTextFrame();

            tf.Width = Unit.FromMillimeter(5);
            tf.Height = Unit.FromPoint(1);

            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;

            tf.Left = Unit.FromMillimeter(0);   // linker Seitenrand
            tf.Top = Unit.FromMillimeter(posMm);

            tf.LineFormat.Width = 0;
            tf.FillFormat.Color = Colors.Black;
        }
    }

    private static void BuildFooter(Section section, InvoicePdfModel model)
    {
        var footer = section.Footers.Primary;

        var paragraph1 = footer.AddParagraph($"Rechnungsdatum: {model.IssueDate:dd.MM.yyyy}");
        paragraph1.AddTab();
        paragraph1.AddText($"Rechnungsnr.: {model.InvoiceNumber}");

        var paragraph2 = footer.AddParagraph($"Name: {model.PatientName}");
        paragraph2.AddTab();
        paragraph2.AddText("");
        paragraph2.AddTab();
        paragraph2.AddText("Seite ");
        paragraph2.AddPageField();
        paragraph2.AddText(" von ");
        paragraph2.AddNumPagesField();
    }

    private static void BuildPatientBlock(Section section, InvoicePdfModel model)
    {
        // Add a TextFrame.
        var textFrame = section.AddTextFrame();
        // size
        textFrame.Width = Unit.FromCentimeter(8.5);
        textFrame.Height = Unit.FromCentimeter(4);
        // position
        textFrame.Left = ShapePosition.Left;
        textFrame.RelativeHorizontal = RelativeHorizontal.Page;
        textFrame.Left = Unit.FromCentimeter(2.2);
        textFrame.Top = ShapePosition.Top;
        textFrame.RelativeVertical = RelativeVertical.Page;
        textFrame.Top = Unit.FromCentimeter(4.4);

        var practiceDetails = textFrame.AddParagraph();
        practiceDetails.Format.Font.Size = 7;
        practiceDetails.Format.Font.Underline = Underline.Single;
        practiceDetails.AddText(model.PracticeName);
        practiceDetails.AddText(" · ");
        practiceDetails.AddText(model.PracticeStreetNr);
        practiceDetails.AddText(" · ");
        practiceDetails.AddText(model.PracticeCityCode);
        practiceDetails.AddLineBreak();
        practiceDetails.AddLineBreak();

        var paragraph = textFrame.AddParagraph();
        paragraph.AddText(model.PatientName);
        paragraph.AddLineBreak();
        paragraph.AddText(model.PatientStreetNr);
        paragraph.AddLineBreak();
        paragraph.AddText(model.PatientCityCode);

        section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.4);
    }

    private static void BuildInvoiceDetailsBlock(Section section, InvoicePdfModel model)
    {
        // Add a TextFrame.
        var textFrame = section.AddTextFrame();
        // size
        textFrame.Width = Unit.FromCentimeter(8);
        textFrame.Height = Unit.FromCentimeter(5);
        // position
        textFrame.Left = ShapePosition.Right;
        textFrame.RelativeHorizontal = RelativeHorizontal.Margin;
        textFrame.Top = ShapePosition.Top;
        textFrame.RelativeVertical = RelativeVertical.Page;
        textFrame.Top = Unit.FromCentimeter(4.4);
        // Add some text.
        textFrame.AddParagraph($"Rechnungsdatum: {model.IssueDate:dd.MM.yyyy}");
        textFrame.AddParagraph($"Rechnungsnummer: {model.InvoiceNumber}");
        textFrame.AddParagraph($"SteuerNr.: {model.TaxIdNumber}");
        textFrame.AddParagraph("");
        textFrame.AddParagraph($"Betr.: {model.Subject}");
        textFrame.AddParagraph($"ICD-Diagnose: {model.Diagnosis}");
    }

    private static void BuildLinesTable(Section section, InvoicePdfModel model)
    {
        var par = section.AddParagraph($"Rechnung vom {model.IssueDate:dd.MM.yyyy}");
        par.Format.Font.Bold = true;
        par.Format.SpaceBefore = Unit.FromCentimeter(4.9);
        par.Format.SpaceAfter = Unit.FromCentimeter(0.5);

        var letter = section.AddParagraph(model.Anrede + $"{model.PatientName},");
        letter.AddLineBreak();
        letter.AddText("für meine Bemühungen erlaube ich mir entsprechend der Gebührenordnung für Psychotherapeut*innen (GOP/GOÄ)");
        letter.AddFormattedText($" {model.TotalAmountEuro.ToString("N2", De)} € ", TextFormat.Bold);
        letter.AddText("zu berechnen. Eine Aufstellung aller berücksichtigten Einzelpositionen finden Sie nachfolgend.");
        letter.Format.SpaceAfter = Unit.FromCentimeter(0.1);

        var table = section.AddTable();
        table.Borders.Width = 0.5;

        table.AddColumn(Unit.FromCentimeter(2.1)); // Datum
        table.AddColumn(Unit.FromCentimeter(1.0)); // Nr of Units
        table.AddColumn(Unit.FromCentimeter(1.6)); // GOP Nr
        table.AddColumn(Unit.FromCentimeter(9.0)); // Bezeichnung
        table.AddColumn(Unit.FromCentimeter(1.5)); // Dauer
        table.AddColumn(Unit.FromCentimeter(2.1)); // Betrag

        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Format.Font.Bold = true;
        header.Shading.Color = Colors.LightGray;

        header.Cells[0].AddParagraph("Datum");
        header.Cells[1].AddParagraph("Anz.");
        header.Cells[2].AddParagraph("GOP Nr");
        header.Cells[3].AddParagraph("Bezeichnung");
        header.Cells[4].AddParagraph("Faktor");
        header.Cells[5].AddParagraph("Betrag");

        foreach (var line in model.Lines)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(line.AppointmentStart.ToString("dd.MM.yyyy", De));
            row.Cells[1].AddParagraph(line.NumberOfUnits.ToString());
            row.Cells[2].AddParagraph(line.GopNr);
            row.Cells[3].AddParagraph(line.Description);
            row.Cells[4].AddParagraph($"{line.Factor}");
            row.Cells[5].AddParagraph($"{line.AmountEuro.ToString("N2", De)} €");
            row.Cells[5].Format.Alignment = ParagraphAlignment.Right;
        }

        {
            table.AddRow();
            var row = table.AddRow();
            row.Cells[0].AddParagraph().AddFormattedText("Gesamt:", TextFormat.Bold);
            row.Cells[5].AddParagraph().AddFormattedText($"{model.TotalAmountEuro.ToString("N2", De)} €", TextFormat.Bold);
        }


        section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.3);
    }

    private static void BuildTotal(Section section, InvoicePdfModel model)
    {
        using var qrCodeData = TryCreatePaymentQrCodeData(model);

        if (qrCodeData is null)
        {
            var paymentDetailsParagraph = section.AddParagraph();
            paymentDetailsParagraph.Format.KeepTogether = true;
            AddPaymentDetails(paymentDetailsParagraph, model);
            paymentDetailsParagraph.AddLineBreak();
            paymentDetailsParagraph.AddLineBreak();
            AddSignature(section, model);
            return;
        }

        var paymentTable = section.AddTable();
        paymentTable.Borders.Width = 0;
        paymentTable.Rows.LeftIndent = 0;
        paymentTable.AddColumn(Unit.FromCentimeter(12.2));
        paymentTable.AddColumn(Unit.FromCentimeter(4.6));

        var row = paymentTable.AddRow();
        row.TopPadding = Unit.FromCentimeter(0.1);
        row.BottomPadding = Unit.FromCentimeter(0.3);
        row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
        row.Cells[1].VerticalAlignment = VerticalAlignment.Top;

        var paymentDetails = row.Cells[0].AddParagraph();
        paymentDetails.Format.KeepTogether = true;
        AddPaymentDetails(paymentDetails, model);

        // var qrCode = row.Cells[1].AddParagraph();
        // qrCode.Format.KeepTogether = true;
        // qrCode.Format.Alignment = ParagraphAlignment.Right;
        // qrCode.AddFormattedText("SEPA-Überweisung", TextFormat.Bold);
        // qrCode.AddLineBreak();
        AddQrCodeTable(row.Cells[1], qrCodeData);

        section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.2);
        AddSignature(section, model);
    }

    private static void AddPaymentDetails(Paragraph paymentDetails, InvoicePdfModel model)
    {
        paymentDetails.AddFormattedText($"Bitte überweisen Sie den Gesamtbetrag innerhalb von {model.PaymentTermInDays} Tagen auf folgendes Konto: ", TextFormat.Bold);
        paymentDetails.AddLineBreak();
        paymentDetails.AddText($"IBAN: {model.Iban}");
        paymentDetails.AddLineBreak();
        paymentDetails.AddText($"BIC: {model.Bic}");
        paymentDetails.AddLineBreak();
        paymentDetails.AddText($"Bank: {model.BankName}");
        paymentDetails.AddLineBreak();
        paymentDetails.AddText($"Betreff: Rechnung {model.InvoiceNumber}");
    }

    private static void AddSignature(Section section, InvoicePdfModel model)
    {
        var signature = section.AddParagraph();
        signature.Format.KeepTogether = true;
        signature.AddText("Mit freundlichen Grüßen");
        signature.AddLineBreak();
        signature.AddLineBreak();
        // signature.AddLineBreak();
        signature.AddFormattedText("\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0", TextFormat.Underline);
        signature.AddLineBreak();
        signature.AddText($"{model.PractitionerTitle} {model.PractitionerName}");
    }

    private static QRCodeData? TryCreatePaymentQrCodeData(InvoicePdfModel model)
    {
        if (!model.IncludePaymentQrCode ||
            model.TotalAmountEuro <= 0m ||
            string.IsNullOrWhiteSpace(model.Iban) ||
            string.IsNullOrWhiteSpace(model.PracticeName))
        {
            return null;
        }

        try
        {
            var payload = new PayloadGenerator.Girocode(
                iban: NormalizeIban(model.Iban),
                bic: NormalizeBic(model.Bic),
                name: LimitLength(model.PractitionerName, 70),
                amount: model.TotalAmountEuro,
                remittanceInformation: LimitLength($"Rechnung {model.InvoiceNumber}", 140));

            return QRCodeGenerator.GenerateQrCode(payload);
        }
        catch
        {
            return null;
        }
    }

    private static void AddQrCodeTable(Cell targetCell, QRCodeData qrCodeData)
    {
        var matrix = qrCodeData.ModuleMatrix;
        var matrixSize = matrix.Count;
        var moduleCount = matrixSize + PaymentQrCodeQuietZoneModules * 2;
        var moduleSize = Unit.FromCentimeter(PaymentQrCodeSizeCentimeters / moduleCount);
        var table = targetCell.Elements.AddTable();

        table.Borders.Width = 0;
        table.Rows.LeftIndent = Unit.FromCentimeter(0.6);

        for (var columnIndex = 0; columnIndex < moduleCount; columnIndex++)
        {
            table.AddColumn(moduleSize);
        }

        for (var rowIndex = 0; rowIndex < moduleCount; rowIndex++)
        {
            var row = table.AddRow();
            row.Height = moduleSize;
            row.HeightRule = RowHeightRule.Exactly;
            row.TopPadding = Unit.FromPoint(0);
            row.BottomPadding = Unit.FromPoint(0);

            for (var columnIndex = 0; columnIndex < moduleCount; columnIndex++)
            {
                var cell = row.Cells[columnIndex];
                cell.Shading.Color = IsDarkQrModule(matrix, rowIndex, columnIndex)
                    ? Colors.Black
                    : Colors.White;
            }
        }
    }

    private static bool IsDarkQrModule(IReadOnlyList<System.Collections.BitArray> matrix, int rowIndex, int columnIndex)
    {
        var matrixRowIndex = rowIndex - PaymentQrCodeQuietZoneModules;
        var matrixColumnIndex = columnIndex - PaymentQrCodeQuietZoneModules;

        if (matrixRowIndex < 0 ||
            matrixColumnIndex < 0 ||
            matrixRowIndex >= matrix.Count ||
            matrixColumnIndex >= matrix[matrixRowIndex].Count)
        {
            return false;
        }

        return matrix[matrixRowIndex][matrixColumnIndex];
    }

    private static string NormalizeIban(string iban)
    {
        return string.Concat(iban.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }

    private static string NormalizeBic(string? bic)
    {
        return string.Concat((bic ?? "").Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }

    private static string LimitLength(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

}
