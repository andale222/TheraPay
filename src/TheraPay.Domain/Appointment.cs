namespace TheraPay.Domain;

public class Appointment
{
    private const int MaxDurationInMinutes = 24 * 60; // 1 Day
    private readonly List<BillingNumber> _billingNumbers = [];

    public Guid Id { get; }
    public DateTime Date { get; private set; }
    public string PatientID { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public int DurationInMinutes { get; private set; }
    public decimal TotalAmount => _billingNumbers.Sum(billingNumber => billingNumber.Amount);
    public IReadOnlyList<BillingNumber> BillingNumbers => _billingNumbers;
    public DateTime End => Date.AddMinutes(DurationInMinutes);

    public Appointment(DateTime date, string patientID)
        : this(Guid.NewGuid(), date, patientID, 0, [])
    {
    }

    private Appointment(Guid id, DateTime date, string patientID, int durationInMinutes, IEnumerable<BillingNumber> billingNumbers)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        Date = date;
        PatientID = patientID;
        Id = id;
        SetDuration(durationInMinutes);
        SetBillingNumbers(billingNumbers);
        Status = AppointmentStatus.Open;
    }

    public static Appointment Rehydrate(Guid id, DateTime date, string patientID, int durationInMinutes, AppointmentStatus status)
    {
        return Rehydrate(id, date, patientID, durationInMinutes, status, []);
    }

    public static Appointment Rehydrate(
        Guid id,
        DateTime date,
        string patientID,
        int durationInMinutes,
        AppointmentStatus status,
        IEnumerable<BillingNumber> billingNumbers)
    {
        var aptmt = new Appointment(id, date, patientID, durationInMinutes, billingNumbers)
        {
            Status = status
        };
        return aptmt;
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

    public void AssignBillingNumber(BillingNumber billingNumber)
    {
        if (billingNumber == null)
        {
            throw new ArgumentNullException(nameof(billingNumber));
        }

        _billingNumbers.Add(billingNumber);
    }

    public void SetBillingNumbers(IEnumerable<BillingNumber> billingNumbers)
    {
        if (billingNumbers == null)
        {
            throw new ArgumentNullException(nameof(billingNumbers));
        }

        var billingNumberList = billingNumbers.ToList();
        _billingNumbers.Clear();
        foreach (var billingNumber in billingNumberList)
        {
            AssignBillingNumber(billingNumber);
        }
    }

    public bool RemoveBillingNumber(string numberIdentifier, BillingNumberType? type = null)
    {
        var billingNumber = _billingNumbers.FirstOrDefault(item =>
            string.Equals(item.NumberIdentifier, numberIdentifier, StringComparison.OrdinalIgnoreCase)
            && (type is null || item.Type == type));

        return billingNumber != null && _billingNumbers.Remove(billingNumber);
    }

    public bool OverlapsWith(Appointment other)
    {
        return Date < other.End && End > other.Date;
    }

    public void SetStatusToBilled()
    {
        Status = AppointmentStatus.Billed;
    }
}


public enum AppointmentStatus { Billed, Open };
