namespace TheraPay.Domain;

public class Appointment
{
    public DateTime Date { get; private set; }
    public Patient Patient { get; private set; }

    public Appointment(DateTime date, Patient patient)
    {
        Date = date;
        Patient = patient;
    }
}