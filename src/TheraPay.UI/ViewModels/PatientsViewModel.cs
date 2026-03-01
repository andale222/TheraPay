using System;
using System.Collections.ObjectModel;
using System.Linq;
using TheraPay.Core;
using TheraPay.Domain;  
using TheraPay.UI.Navigation;

namespace TheraPay.UI.ViewModels;

public class PatientsViewModel : ViewModelBase
{
    private readonly InMemoryPatientRepository _store;
    public RelayCommand NavigateHomeViewCommand  { get; }

    public ObservableCollection<Patient> Patients { get; } = new();

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); }
    }

    private string _lastName = "";
    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); }
    }
    private string _patientID = "";
    public string PatientID
{
    get => _patientID;
    set
    {
        if (_patientID != value)
        {
            _patientID = value;
            OnPropertyChanged();
        }
    }
}

    public PatientsViewModel(PatientService patientService, InMemoryPatientRepository store, NavigationService nav)
    {
        _store = store;
        NavigateHomeViewCommand = new RelayCommand(() => nav.NavigateTo<HomeViewModel>());
        Reload();
    }

    public void AddPatient()
    {
        var p = new Patient(FirstName.Trim(), LastName.Trim(),PatientID);
        _store.Add(p);

        FirstName = "";
        LastName = "";
        PatientID = "";
        Reload();
    }

    private void Reload()
    {
        Patients.Clear();
        foreach (var p in _store.GetAll().OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            Patients.Add(p);
    }
}