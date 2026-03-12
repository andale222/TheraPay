namespace TheraPay.UI.State;

public sealed class ProjectSession
{
    private string _patientListPath = "";
    private bool _hasUnsavedChanges;

    public string PatientListPath => _patientListPath;
    public bool HasPatientListPath => !string.IsNullOrWhiteSpace(_patientListPath);
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public void SetPatientListPath(string path)
    {
        _patientListPath = path.Trim();
    }

    public void MarkUnsavedChanges()
    {
        _hasUnsavedChanges = true;
    }

    public void MarkSaved()
    {
        _hasUnsavedChanges = false;
    }
}
