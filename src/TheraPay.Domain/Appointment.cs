namespace TheraPay.Domain;

public class Appointment
{
    private const int MaxDurationInMinutes = 24*60; // 1 Day
    public Guid Id { get; }
    public DateTime Date { get; private set; }
    public string PatientID { get; private set; }
    public int DurationInMinutes { get; private set; }
    public DateTime End => Date.AddMinutes(DurationInMinutes);

    public Appointment(DateTime date, string patientID)
    {
        Date = date;
        PatientID = patientID;
        Id = Guid.NewGuid();
    }

    public void SetDuration(int durationInMinutes)
    {
        if (durationInMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationInMinutes), "Duration cannot be negative.");
        }
        if (durationInMinutes > MaxDurationInMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(durationInMinutes), $"Duration cannot exceed {MaxDurationInMinutes} minutes.");
        }
        DurationInMinutes = durationInMinutes;
    }
}