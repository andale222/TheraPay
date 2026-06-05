using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;

namespace TheraPay.UI.ViewModels;

public class PatientFields
{
    public string PatientID { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string ICD10Diagnosis { get; set; } = "";
    public string Street { get; set; } =  "";
    public string HouseNumber { get; set; } =  "";
    public string PostalCode { get; set; } =  "";
    public string Place { get; set; } =  "";
    public string Country { get; set; } =  "";
    public string Email { get; set; } =  "";
    public string PhoneNumber { get; set; } =  "";
    public string AdditionalInfo { get; set; } =  "";
    public bool IsActive { get; set; } =  true;
    public string InsuranceStatus { get; set; } =  "Privat";
    public bool IsInactive
    {
        get => IsActive == false;
        set
        {
            if (value)
                IsActive = false;
        }
    }

    public ObservableCollection<string> InsuranceStatusSelection { get; } = new()
    {
        "Privat", "GKV", "Selbstzahler", "Kostenerstattung"
    };
}
public class PatientsViewModel : ViewModelBase
{
    private readonly PatientService _patientService;
    private readonly ProjectSession _session;
    public RelayCommand NavigateHomeViewCommand  { get; }
    public RelayCommand AddPatientCommand  { get; }
    public RelayCommand CheckDataCommand  { get; }

    public ObservableCollection<Patient> Patients { get; } = new();
    public IReadOnlyList<PatientInsuranceStatus> InsuranceStatuses { get; } = Enum.GetValues<PatientInsuranceStatus>();

    private PatientFields _patientFields = new();
    public PatientFields PatientFields
    {
        get => _patientFields;
        private set
        {
            _patientFields = value;
            OnPropertyChanged();
        }
    }

    public PatientsViewModel(PatientService patientService, ProjectSession session, NavigationService nav)
    {
        _patientService = patientService;
        _session = session;
        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        AddPatientCommand = new RelayCommand(AddPatient);
        CheckDataCommand = new RelayCommand(CheckData);
        Reload();
    }

    private void AddPatient()
    {
        var addResult = _patientService.AddPatient(
            PatientFields.FirstName,
            PatientFields.LastName,
            PatientFields.PatientID,
            PatientFields.Street,
            PatientFields.HouseNumber,
            PatientFields.PostalCode,
            PatientFields.Place,
            PatientFields.Country,
            PatientFields.AdditionalInfo,
            PatientFields.Email,
            PatientFields.PhoneNumber,
            PatientFields.ICD10Diagnosis,
            PatientFields.InsuranceStatus,
            PatientFields.IsActive);

        if (addResult.Ok)
        {
            _session.MarkUnsavedChanges();
            PatientFields = new PatientFields();
        }

        Reload();

        NavigateHomeViewCommand.Execute(null);
    }

    private void CheckData()
    {
        // TODO: Implement data checking logic here
    }

    private void Reload()
    {
        Patients.Clear();
        foreach (var p in _patientService.ViewPatients().OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            Patients.Add(p);
    }
}
