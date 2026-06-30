using System;
using System.Windows.Input;
using TheraPay.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using TheraPay.UI.ViewModels.Panels;
using TheraPay.UI.Services;

namespace TheraPay.UI.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private readonly NavigationStore _store;
    private readonly ProjectPersistenceService _projectPersistence;
    public PatientPanelViewModel PatientsPanel { get; }
    public CalendarPanelViewModel CalendarPanel { get; }

    public object? CurrentViewModel => _store.CurrentViewModel;
    public ICommand NavigatePatientsCommand  { get; }
    public ICommand NavigateEditAppointmentCommand  { get; }
    public ICommand NavigateInvoiceCreationCommand  { get; }
    public ICommand NavigateInvoicesCommand  { get; }
    public ICommand SaveDataCommand  { get; }

    private string _saveStatusMessage = "";
    public string SaveStatusMessage
    {
        get => _saveStatusMessage;
        private set
        {
            if (_saveStatusMessage == value) return;
            _saveStatusMessage = value;
            OnPropertyChanged();
        }
    }

    public HomeViewModel(
        PatientPanelViewModel patientsPanel,
        CalendarPanelViewModel calendarPanel,
        NavigationStore store,
        NavigationService nav,
        ProjectPersistenceService projectPersistence)
    {
        PatientsPanel = patientsPanel;
        CalendarPanel = calendarPanel;
        _projectPersistence = projectPersistence;
        _store = store;
        _store.CurrentViewModelChanged += (_, __) => OnPropertyChanged(nameof(CurrentViewModel));

        NavigatePatientsCommand  = new RelayCommand(() => nav.NavigateTo<PatientsViewModel>());
        NavigateEditAppointmentCommand  = new RelayCommand(() => nav.NavigateTo<AppointmentEditViewModel>(vm =>
        {
            if (PatientsPanel.SelectedPatient?.Id is not null)
                vm.SelectPatient(PatientsPanel.SelectedPatient.Id);
        }));
        NavigateInvoiceCreationCommand  = new RelayCommand(() => nav.NavigateTo<InvoiceCreationViewModel>(vm =>
        {
            if (PatientsPanel.SelectedPatient?.Id is not null)
                vm.SelectPatient(PatientsPanel.SelectedPatient.Id);
        }));
        NavigateInvoicesCommand  = new RelayCommand(() => nav.NavigateTo<InvoicesViewModel>());
        SaveDataCommand  = new RelayCommand(Save);
    }

    private void Save()
    {
        var result = _projectPersistence.SaveProject();
        SaveStatusMessage = result.Ok
            ? $"Zuletzt gespeichert: {DateTime.Now:HH:mm:ss}"
            : result.Error ?? "Speichern fehlgeschlagen.";
    }

}
