namespace TheraPay.Domain;

public class InvoiceNumberState
{
    private const int MinRandomStart = 500;
    private const int MaxRandomStartExclusive = 3000;

    public int Year { get; private set; }
    public int RandomStart { get; private set; }
    public int NextIssueNumber { get; private set; }

    public InvoiceNumberState()
        : this(DateTime.Today.Year, Random.Shared.Next(MinRandomStart, MaxRandomStartExclusive), 1)
    {
    }

    private InvoiceNumberState(int year, int randomStart, int nextIssueNumber)
    {
        Year = year;
        RandomStart = randomStart;
        NextIssueNumber = nextIssueNumber;
        EnsureValidState();
    }

    public static InvoiceNumberState Rehydrate(int year, int randomStart, int nextIssueNumber)
    {
        return new InvoiceNumberState(year, randomStart, nextIssueNumber);
    }

    public int ConsumeNextSerial(DateTime issueDate)
    {
        if (Year != issueDate.Year)
        {
            Year = issueDate.Year;
            RandomStart = Random.Shared.Next(MinRandomStart, MaxRandomStartExclusive);
            NextIssueNumber = 1;
        }

        EnsureValidState();

        var serial = RandomStart + NextIssueNumber;
        if (serial > 9999)
        {
            throw new InvalidOperationException("Invoice number range exceeded for the current year.");
        }

        NextIssueNumber++;
        return serial;
    }

    private void EnsureValidState()
    {
        if (Year < 1)
        {
            throw new InvalidOperationException("Year must be greater than 0.");
        }

        if (RandomStart < MinRandomStart || RandomStart >= MaxRandomStartExclusive)
        {
            throw new InvalidOperationException(
                $"RandomStart must be in range [{MinRandomStart}, {MaxRandomStartExclusive - 1}]");
        }

        if (NextIssueNumber < 1)
        {
            throw new InvalidOperationException("NextIssueNumber must be greater than 0.");
        }
    }
}
