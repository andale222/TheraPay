namespace TheraPay.Core.Tests;

using TheraPay.Core;
using TheraPay.Domain;

public static class TestData
{

    public static Patient Patient1( ) => new Patient("A", "J", "L5R");
    public static Patient Patient2( ) => new Patient("second", "patient", "NR2");
}