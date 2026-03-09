using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv;

public sealed class PatientCsvRecord
{
    public string Id { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public bool IsDeleted { get; set; }
}
