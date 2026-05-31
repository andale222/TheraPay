namespace TheraPay.UI.State;
using TheraPay.Domain;

public sealed class ProjectSession
{
    private string _patientListPath = "";
    private string _appointmentListPath = "";
    private string _practiceDataPath = "";
    private PracticeData _practiceData = new PracticeData();
    private bool _hasUnsavedChanges;

    public PracticeData PracticeData => _practiceData;
    public string PatientListPath => _patientListPath;
    public string AppointmentListPath => _appointmentListPath;
    public string PracticeDataPath => _practiceDataPath;
    public bool HasPatientListPath => !string.IsNullOrWhiteSpace(_patientListPath);
    public bool HasAppointmentListPath => !string.IsNullOrWhiteSpace(_appointmentListPath);
    public bool HasPracticeDataPath => !string.IsNullOrWhiteSpace(_practiceDataPath);
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public void SetPatientListPath(string path)
    {
        _patientListPath = path.Trim();
    }

    public void SetAppointmentListPath(string path)
    {
        _appointmentListPath = path.Trim();
    }
    public void SetPracticeDataPath(string path)
    {
        _practiceDataPath = path.Trim();
    }

    public void MarkUnsavedChanges()
    {
        _hasUnsavedChanges = true;
    }

    public void MarkSaved()
    {
        _hasUnsavedChanges = false;
    }

    public void SetPracticeData(PracticeData practiceData)
    {
        if (practiceData != null)
        {
            _practiceData = practiceData;
            MarkUnsavedChanges();
        }
    }
}
