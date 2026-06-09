namespace TheraPay.UI.Tests;

using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;

public class PatientsViewModel_test
{
    [Fact]
    public void GivenNewPatientView_PatientDataIsEditable()
    {
        // GIVEN
        PatientsViewModel viewModel = CreateViewModel();

        // THEN
        Assert.False(viewModel.IsEditMode);
        Assert.True(viewModel.IsPatientDataEditable);
        Assert.True(viewModel.IsPatientIdEditable);
    }

    [Fact]
    public void GivenExistingPatient_LoadPatientForEdit_DisablesPatientDataEditing()
    {
        // GIVEN
        var patientRepository = new InMemoryPatientRepository();
        patientRepository.Add(new Patient("Ada", "Lovelace", "AL1"));
        PatientsViewModel viewModel = CreateViewModel(patientRepository);

        // WHEN
        viewModel.LoadPatientForEdit("AL1");

        // THEN
        Assert.True(viewModel.IsEditMode);
        Assert.False(viewModel.IsPatientDataEditable);
        Assert.False(viewModel.IsPatientIdEditable);
        Assert.Equal("AL1", viewModel.PatientFields.PatientID);
    }

    [Fact]
    public void GivenExistingPatient_WhenEditingEnabled_PatientIdStaysLocked()
    {
        // GIVEN
        var patientRepository = new InMemoryPatientRepository();
        patientRepository.Add(new Patient("Ada", "Lovelace", "AL1"));
        PatientsViewModel viewModel = CreateViewModel(patientRepository);
        viewModel.LoadPatientForEdit("AL1");

        // WHEN
        viewModel.IsPatientDataEditable = true;

        // THEN
        Assert.True(viewModel.IsPatientDataEditable);
        Assert.False(viewModel.IsPatientIdEditable);
    }

    private static PatientsViewModel CreateViewModel(InMemoryPatientRepository? patientRepository = null)
    {
        patientRepository ??= new InMemoryPatientRepository();
        var patientService = new PatientService(patientRepository);
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());

        return new PatientsViewModel(patientService, new ProjectSession(), navigationService);
    }
}
