using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TheraPay.UI.ViewModels;

public partial class PatientFields : ObservableValidator
{
    public string PatientID { get; set; } = "";
    public string Salutation { get; set; } = PatientSalutation.Options[0];
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RegularExpression(Patient.Icd10DiagnosisPattern,
        ErrorMessage = "Ungültige ICD-10 Diagnose")]
    private string icd10Diagnosis =  "";
    public string Street { get; set; } =  "";
    public string HouseNumber { get; set; } =  "";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RegularExpression(Address.PostalCodePattern,
        ErrorMessage = "Postleitzahl muss aus 5 Ziffern bestehen")]
    private string postalCode =  "";
    public string Place { get; set; } =  "";
    public string Country { get; set; } =  "";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RegularExpression(Patient.EmailPattern,
        ErrorMessage = "Ungültige E-Mail-Adresse")]
    private string email =  "";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [RegularExpression(Patient.PhoneNumberPattern,
        ErrorMessage = "Ungültige Telefonnummer")]
    private string phoneNumber =  "";
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

    public ObservableCollection<string> SalutationSelection { get; } = new(PatientSalutation.Options);

    public static PatientFields FromPatient(Patient patient)
    {
        return new PatientFields
        {
            PatientID = patient.ID,
            Salutation = string.IsNullOrWhiteSpace(patient.Salutation) ? PatientSalutation.Options[0] : patient.Salutation,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
            Icd10Diagnosis = patient.ICD10Diagnosis,
            Street = patient.Address?.Street ?? "",
            HouseNumber = patient.Address?.HouseNumber ?? "",
            PostalCode = patient.Address?.PostalCode ?? "",
            Place = patient.Address?.City ?? "",
            Country = patient.Address?.Country ?? "",
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            AdditionalInfo = patient.Address?.Additional ?? "",
            IsActive = patient.IsActive,
            InsuranceStatus = patient.InsuranceStatus.ToString()
        };
    }
}
public class PatientsViewModel : ViewModelBase
{
    private readonly PatientService _patientService;
    private readonly ProjectSession _session;
    private string? _editingPatientId;
    public RelayCommand NavigateHomeViewCommand  { get; }
    public RelayCommand AddPatientCommand  { get; }
    public RelayCommand CheckDataCommand  { get; }

    public ObservableCollection<Patient> Patients { get; } = new();
    public IReadOnlyList<PatientInsuranceStatus> InsuranceStatuses { get; } = Enum.GetValues<PatientInsuranceStatus>();
    public bool IsEditMode => _editingPatientId is not null;
    public bool IsPatientIdEditable => IsEditMode == false && IsPatientDataEditable;
    public string PatientFormTitle => IsEditMode ? "Patient bearbeiten" : "Patient hinzufügen";
    public string SavePatientButtonText => IsEditMode ? "Änderungen speichern" : "Hinzufügen";

    private bool _isPatientDataEditable = true;
    public bool IsPatientDataEditable
    {
        get => _isPatientDataEditable;
        set
        {
            if (_isPatientDataEditable == value)
                return;

            _isPatientDataEditable = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPatientIdEditable));
        }
    }

    private string _checkDataMessage = "";
    public string CheckDataMessage
    {
        get => _checkDataMessage;
        private set
        {
            if (_checkDataMessage == value)
                return;

            _checkDataMessage = value;
            OnPropertyChanged();
        }
    }

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
        AddPatientCommand = new RelayCommand(SavePatient);
        CheckDataCommand = new RelayCommand(CheckData);
        Reload();
    }

    public void LoadPatientForEdit(string patientId)
    {
        Patient? patient = _patientService.FindPatientById(patientId);
        if (patient is null)
        {
            _editingPatientId = null;
            PatientFields = new PatientFields();
            IsPatientDataEditable = true;
            CheckDataMessage = $"Patient mit ID '{patientId}' wurde nicht gefunden.";
            NotifyEditModeChanged();
            return;
        }

        _editingPatientId = patient.ID;
        PatientFields = PatientFields.FromPatient(patient);
        IsPatientDataEditable = false;
        CheckDataMessage = "";
        NotifyEditModeChanged();
    }

    private void SavePatient()
    {
        if (IsEditMode)
            UpdatePatient();
        else
            AddPatient();
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
            PatientFields.Icd10Diagnosis,
            PatientFields.InsuranceStatus,
            PatientFields.IsActive,
            PatientFields.Salutation,
            ToDateOnly(PatientFields.DateOfBirth));

        if (addResult.Ok)
        {
            _session.MarkUnsavedChanges();
            PatientFields = new PatientFields();
            CheckDataMessage = "Patientendaten wurden hinzugefügt.";
        }
        else
        {
            CheckDataMessage = addResult.Error ?? "Patientendaten konnten nicht hinzugefügt werden.";
        }

        Reload();

        if (addResult.Ok)
            NavigateHomeViewCommand.Execute(null);
    }

    private void UpdatePatient()
    {
        string patientId = _editingPatientId ?? PatientFields.PatientID;
        var updateResult = _patientService.UpdatePatient(
            patientId,
            PatientFields.FirstName,
            PatientFields.LastName,
            PatientFields.Street,
            PatientFields.HouseNumber,
            PatientFields.PostalCode,
            PatientFields.Place,
            PatientFields.Country,
            PatientFields.AdditionalInfo,
            PatientFields.Email,
            PatientFields.PhoneNumber,
            PatientFields.Icd10Diagnosis,
            PatientFields.InsuranceStatus,
            PatientFields.IsActive,
            PatientFields.Salutation,
            ToDateOnly(PatientFields.DateOfBirth));

        if (updateResult.Ok)
        {
            _session.MarkUnsavedChanges();
            CheckDataMessage = "Patientendaten wurden aktualisiert.";
        }
        else
        {
            CheckDataMessage = updateResult.Error ?? "Patientendaten konnten nicht aktualisiert werden.";
        }

        Reload();

        if (updateResult.Ok)
            NavigateHomeViewCommand.Execute(null);
    }

    private void CheckData()
    {
        Result result = _patientService.CheckPatientData(
                PatientFields.PatientID,
                PatientFields.Email,
                PatientFields.PhoneNumber,
                PatientFields.PostalCode,
                PatientFields.Icd10Diagnosis);

        CheckDataMessage = result.Ok
            ? IsEditMode ? "Patientendaten können aktualisiert werden." : "Patientendaten können hinzugefügt werden."
            : result.Error ?? "Patientendaten sind nicht vollständig gültig.";
    }

    private void Reload()
    {
        Patients.Clear();
        foreach (var p in _patientService.ViewPatients().OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            Patients.Add(p);
    }

    private static DateOnly? ToDateOnly(DateTime? date)
    {
        return date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    }

    private void NotifyEditModeChanged()
    {
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsPatientIdEditable));
        OnPropertyChanged(nameof(PatientFormTitle));
        OnPropertyChanged(nameof(SavePatientButtonText));
    }
}
