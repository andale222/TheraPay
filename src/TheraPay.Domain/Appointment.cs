namespace TheraPay.Domain;

public class Appointment
{
    private const int MaxDurationInMinutes = 24*60; // 1 Day
    public Guid Id { get; }
    public DateTime Date { get; private set; }
    public string PatientID { get; private set; }
    public int DurationInMinutes { get; private set; }
    public decimal TotalAmount {get; private set; }
    public DateTime End => Date.AddMinutes(DurationInMinutes);

    public Appointment(DateTime date, string patientID)
        : this(Guid.NewGuid(), date, patientID, 0)
    {
    }

    private Appointment(Guid id, DateTime date, string patientID, int durationInMinutes)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        Date = date;
        PatientID = patientID;
        Id = id;
        SetDuration(durationInMinutes);
        TotalAmount = 1.234m;
    }

    public static Appointment Rehydrate(Guid id, DateTime date, string patientID, int durationInMinutes)
    {
        return new Appointment(id, date, patientID, durationInMinutes);
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

    public bool OverlapsWith(Appointment other)
    {
        return Date < other.End && End > other.Date;
    }
}
