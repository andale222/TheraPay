using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public ObservableCollection<string> InsuranceStatusSelection { get; } = new()
    {
        "Privat", "Selbstzahler", "Kostenerstattung"
    };
}
public class PatientsViewModel : ViewModelBase
{
    private readonly IPatientRepository _store;
    private readonly ProjectSession _session;
    public RelayCommand NavigateHomeViewCommand  { get; }
    public RelayCommand AddPatientCommand  { get; }
    public RelayCommand CheckDataCommand  { get; }

    public ObservableCollection<Patient> Patients { get; } = new();
    public IReadOnlyList<PatientInsuranceStatus> InsuranceStatuses { get; } = Enum.GetValues<PatientInsuranceStatus>();

    public PatientFields PatientFields { get; } = new();

    public PatientsViewModel(PatientService patientService, IPatientRepository store, ProjectSession session, NavigationService nav)
    {
        _store = store;
        _session = session;
        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        AddPatientCommand = new RelayCommand(AddPatient);
        CheckDataCommand = new RelayCommand(CheckData);
        Reload();
    }

    private void AddPatient()
    {
        var patient = new Patient(PatientFields.FirstName.Trim(), PatientFields.LastName.Trim(), PatientFields.PatientID.Trim());
        patient.IsActive = PatientFields.IsActive;
        // patient.SetInsuranceStatus(PatientFields.InsuranceStatus);

        // if (HasCompleteAddress())
        //     patient.SetAddress(Street, HouseNumber, PostalCode, Place, Country, AdditionalInfo);
        // if (string.IsNullOrWhiteSpace(ICD10Diagnosis) == false)
        //     patient.SetICD10Diagnosis(ICD10Diagnosis);
        // if (string.IsNullOrWhiteSpace(Email) == false)
        //     patient.SetEmail(Email);
        // if (string.IsNullOrWhiteSpace(PhoneNumber) == false)
        //     patient.SetPhoneNumber(PhoneNumber);

        var addResult = _store.Add(patient);
        if (addResult.Ok)
        {
            _session.MarkUnsavedChanges();
        }

        Reload();

        NavigateHomeViewCommand.Execute(null);
    }

    private void CheckData()
    {
        // TODO: Implement data checking logic here
    }

    // private bool HasCompleteAddress()
    // {
    //     return string.IsNullOrWhiteSpace(Street) == false
    //         && string.IsNullOrWhiteSpace(HouseNumber) == false
    //         && string.IsNullOrWhiteSpace(PostalCode) == false
    //         && string.IsNullOrWhiteSpace(Place) == false;
    // }

    private void Reload()
    {
        Patients.Clear();
        foreach (var p in _store.GetAll().OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            Patients.Add(p);
    }
}
