namespace TheraPay.UI.State;

public sealed class ProjectSession
{
    private string _patientListPath = "";
    private string _appointmentListPath = "";
    private bool _hasUnsavedChanges;

    public string PatientListPath => _patientListPath;
    public string AppointmentListPath => _appointmentListPath;
    public bool HasPatientListPath => !string.IsNullOrWhiteSpace(_patientListPath);
    public bool HasAppointmentListPath => !string.IsNullOrWhiteSpace(_appointmentListPath);
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public void SetPatientListPath(string path)
    {
        _patientListPath = path.Trim();
    }

    public void SetAppointmentListPath(string path)
    {
        _appointmentListPath = path.Trim();
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
