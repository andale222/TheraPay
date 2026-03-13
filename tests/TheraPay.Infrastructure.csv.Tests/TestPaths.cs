using System.Runtime.CompilerServices;

namespace TheraPay.Infrastructure.csv.Tests;

internal static class TestPaths
{
    public static string DataFile(string fileName, [CallerFilePath] string callerFilePath = "")
    {
        var projectDir = Path.GetDirectoryName(callerFilePath)!;
        return Path.Combine(projectDir, "testData", fileName);
    }
}
