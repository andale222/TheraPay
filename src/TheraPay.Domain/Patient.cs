namespace TheraPay.Domain;

public class Patient
{
    public string LastName { get; private set; } = "";
    public string FirstName { get; private set; } = "";
    public string ID { get; init; } = "";


    public Patient(string firstName, string lastName, string id)
    {
        FirstName = firstName;
        LastName = lastName;
        ID = id;
    }
}
