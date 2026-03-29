using MigraDoc;
using PdfSharp.Fonts;
using PdfCapabilities = PdfSharp.Capabilities;

namespace TheraPay.Infrastructure.Export.Pdf;

public static class PdfFontBootstrap
{
    private static bool _isConfigured;

    public static void Configure()
    {
        if (_isConfigured)
            return;

        if (PdfCapabilities.Build.IsCoreBuild)
        {
            GlobalFontSettings.FontResolver = new TheraPayFontResolver();
        }

        _isConfigured = true;
    }
}