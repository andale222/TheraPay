namespace TheraPay.Infrastructure.csv;

public sealed class AppointmentCsvRecord
{
    public string Id { get; set; } = "";
    public string StartDateTime { get; set; } = "";
    public string Duration { get; set; } = "";
    public string PatientId { get; set; } = "";
    public bool IsDeleted { get; set; }
}
