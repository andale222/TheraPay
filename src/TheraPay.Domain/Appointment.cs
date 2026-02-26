namespace TheraPay.Domain;

public class Appointment
{
    public DateTime Date { get; private set; }
    public string PatientID { get; private set; }

    public Appointment(DateTime date, string patientID)
    {
        Date = date;
        PatientID = patientID;
    }
}