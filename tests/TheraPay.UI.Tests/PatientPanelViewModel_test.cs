namespace TheraPay.UI.Tests;

using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.Services;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels.Panels;

public class PatientPanelViewModel_test
{
    [Fact]
    public void GivenActiveAndInactivePatients_FilterActiveAndInactive_UsesPatientActivityStatus()
    {
        // GIVEN
        var panel = CreatePanelWithActiveAndInactivePatients();

        // WHEN
        panel.FilterAll = true;

        // THEN
        Assert.Equal(2, panel.Patients.Count);

        // WHEN
        panel.FilterActive = true;

        // THEN
        var activePatient = Assert.Single(panel.Patients);
        Assert.Equal("ACTIVE", activePatient.Id);

        // WHEN
        panel.FilterArchived = true;

        // THEN
        var inactivePatient = Assert.Single(panel.Patients);
        Assert.Equal("INACTIVE", inactivePatient.Id);
    }

    [Fact]
    public void GivenOnlyActivePatientsMode_PanelShowsOnlyActivePatientsAndHidesActivityFilter()
    {
        // GIVEN
        var panel = CreatePanelWithActiveAndInactivePatients();

        // WHEN
        panel.ShowOnlyActivePatients = true;

        // THEN
        Assert.False(panel.ShowActivityFilter);
        Assert.True(panel.FilterActive);
        var activePatient = Assert.Single(panel.Patients);
        Assert.Equal("ACTIVE", activePatient.Id);

        // WHEN
        panel.FilterAll = true;
        panel.FilterArchived = true;

        // THEN
        Assert.False(panel.FilterAll);
        Assert.False(panel.FilterArchived);
        activePatient = Assert.Single(panel.Patients);
        Assert.Equal("ACTIVE", activePatient.Id);
    }

    [Fact]
    public async Task GivenSelectedPatient_DeleteSelectedPatientAsync_WhenConfirmed_SoftDeletesAndRemovesPatient()
    {
        // GIVEN
        var patientRepository = new InMemoryPatientRepository();
        patientRepository.Add(new Patient("Ada", "Active", "ACTIVE"));
        var patientService = new PatientService(patientRepository);
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var messageBox = new ConfirmingMessageBoxService(confirm: true);
        var session = new ProjectSession();
        var panel = new PatientPanelViewModel(patientService, navigationService, messageBox, session);
        panel.SelectPatient("ACTIVE");

        // WHEN
        await panel.DeleteSelectedPatientAsync();

        // THEN
        Patient patient = patientRepository.GetById("ACTIVE");
        Assert.True(patient.IsDeleted);
        Assert.False(patient.IsActive);
        Assert.Empty(panel.Patients);
        Assert.True(session.HasUnsavedChanges);
        Assert.Equal(1, messageBox.ConfirmationCount);
        Assert.Contains("nicht widerrufbar", messageBox.LastConfirmationMessage);
    }

    [Fact]
    public async Task GivenSelectedPatient_DeleteSelectedPatientAsync_WhenCancelled_KeepsPatient()
    {
        // GIVEN
        var patientRepository = new InMemoryPatientRepository();
        patientRepository.Add(new Patient("Ada", "Active", "ACTIVE"));
        var patientService = new PatientService(patientRepository);
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var messageBox = new ConfirmingMessageBoxService(confirm: false);
        var session = new ProjectSession();
        var panel = new PatientPanelViewModel(patientService, navigationService, messageBox, session);
        panel.SelectPatient("ACTIVE");

        // WHEN
        await panel.DeleteSelectedPatientAsync();

        // THEN
        Patient patient = patientRepository.GetById("ACTIVE");
        Assert.False(patient.IsDeleted);
        Assert.Single(panel.Patients);
        Assert.False(session.HasUnsavedChanges);
        Assert.Equal(1, messageBox.ConfirmationCount);
    }

    private static PatientPanelViewModel CreatePanelWithActiveAndInactivePatients()
    {
        var patientRepository = new InMemoryPatientRepository();
        patientRepository.Add(new Patient("Ada", "Active", "ACTIVE"));

        var inactivePatient = new Patient("Ida", "Inactive", "INACTIVE");
        inactivePatient.IsActive = false;
        patientRepository.Add(inactivePatient);

        var patientService = new PatientService(patientRepository);
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());

        return new PatientPanelViewModel(patientService, navigationService);
    }

    private sealed class ConfirmingMessageBoxService : IMessageBoxService
    {
        private readonly bool _confirm;
        public int ConfirmationCount { get; private set; }
        public string LastConfirmationMessage { get; private set; } = "";

        public ConfirmingMessageBoxService(bool confirm)
        {
            _confirm = confirm;
        }

        public Task ShowErrorAsync(string title, string message)
        {
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmWarningAsync(string title, string message, string confirmText = "OK", string cancelText = "Abbrechen")
        {
            ConfirmationCount++;
            LastConfirmationMessage = message;
            return Task.FromResult(_confirm);
        }
    }
}
