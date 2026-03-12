namespace TheraPay.UI.State;

public sealed class ProjectSession
{
    private string _patientListPath = "";

    public string PatientListPath => _patientListPath;
    public bool HasPatientListPath => !string.IsNullOrWhiteSpace(_patientListPath);

    public void SetPatientListPath(string path)
    {
        _patientListPath = path.Trim();
    }
}
