using PdfSharp.Fonts;

namespace TheraPay.Infrastructure.Export.Pdf;

public sealed class TheraPayFontResolver : IFontResolver
{
    private const string FamilyName = "TheraSans";

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        // Alles auf eine kontrollierte Schriftfamilie mappen
        if (bold)
            return new FontResolverInfo("TheraSans-Bold");

        return new FontResolverInfo("TheraSans-Regular");
    }

    public byte[]? GetFont(string faceName)
    {
        var fontDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");

        return faceName switch
        {
            "TheraSans-Regular" => File.ReadAllBytes(Path.Combine(fontDir, "SourceSerif4-Regular.ttf")),
            "TheraSans-Bold"    => File.ReadAllBytes(Path.Combine(fontDir, "SourceSerif4-Bold.ttf")),
            _ => null
        };
    }
}