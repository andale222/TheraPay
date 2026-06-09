namespace TheraPay.UI.Tests;

using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
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
}
