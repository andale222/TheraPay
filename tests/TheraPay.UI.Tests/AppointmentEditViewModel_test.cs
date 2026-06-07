namespace TheraPay.UI.Tests;

using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.ViewModels;
using TheraPay.UI.ViewModels.Panels;
using TheraPay.UI.State;

public class AppointmentEditViewModel_test
{
    [Fact]
    public void Test1()
    {

    }

    [Fact]
    public void GivenSelectedBillingNumber_EditDraftAndAdd_AssignsEditedBillingNumber()
    {
        // GIVEN
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var appointmentService = new AppointmentService(appointmentRepository);
        var patientService = new PatientService(patientRepository);
        var viewModel = new AppointmentEditViewModel(
            appointmentService,
            new CalendarPanelViewModel(navigationService, appointmentService, new ProjectSession()),
            new PatientPanelViewModel(patientService, navigationService),
            navigationService,
            new ProjectSession());

        // WHEN
        viewModel.DraftDescription = "Bearbeitete Sprechstunde";
        viewModel.DraftFactor = "2.5";
        viewModel.DraftBaseValue = "80";
        viewModel.AddBillingNumberCommand.Execute(null);

        // THEN
        Assert.Single(viewModel.AssignedBillingNumbers);
        var billingNumber = viewModel.AssignedBillingNumbers[0];
        Assert.Equal(viewModel.SelectedBillingNumber!.NumberIdentifier, billingNumber.NumberIdentifier);
        Assert.Equal("Bearbeitete Sprechstunde", billingNumber.Description);
        Assert.Equal(2.5m, billingNumber.Factor);
        Assert.Equal(80m, billingNumber.BaseValue);
        Assert.Equal(200m, billingNumber.Amount);
    }

    [Fact]
    public void GivenExistingAppointment_LoadAppointmentForEdit_FillsAppointmentFields()
    {
        // GIVEN
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patientRepository.Add(patient);
        var billingNumber = new BillingNumber("801a", 2.5m, 80m, "Bearbeitete Sprechstunde", BillingNumberType.Privat);
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), patient.ID);
        appointment.SetDuration(75);
        appointment.AssignBillingNumber(billingNumber);
        appointmentRepository.Add(appointment);
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var appointmentService = new AppointmentService(appointmentRepository);
        var patientService = new PatientService(patientRepository);
        var viewModel = new AppointmentEditViewModel(
            appointmentService,
            new CalendarPanelViewModel(navigationService, appointmentService, new ProjectSession()),
            new PatientPanelViewModel(patientService, navigationService),
            navigationService,
            new ProjectSession());

        // WHEN
        viewModel.LoadAppointmentForEdit(appointment.Id);

        // THEN
        Assert.True(viewModel.IsEditing);
        Assert.Equal(new DateTime(2026, 1, 1), viewModel.StartDate);
        Assert.Equal(new TimeSpan(14, 0, 0), viewModel.StartTime);
        Assert.Equal(new TimeSpan(15, 15, 0), viewModel.EndTime);
        Assert.Equal(patient.ID, viewModel.PatientsPanel.SelectedPatient!.Id);
        Assert.Single(viewModel.AssignedBillingNumbers);
        Assert.Equal(billingNumber, viewModel.AssignedBillingNumbers[0]);
        Assert.Equal("Bearbeitete Sprechstunde", viewModel.DraftDescription);
        Assert.Equal(2.5m, viewModel.AssignedBillingNumbers[0].Factor);
        Assert.Equal(80m, viewModel.AssignedBillingNumbers[0].BaseValue);
    }

    [Fact]
    public void GivenSelectedAppointment_DeleteCommandRequiresSecondClick()
    {
        // GIVEN
        var appointmentRepository = new InMemoryAppointmentRepository();
        var appointment = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "Pat1");
        appointmentRepository.Add(appointment);
        var appointmentService = new AppointmentService(appointmentRepository);
        var session = new ProjectSession();
        var navigationService = new NavigationService(new NavigationStore(), new ServiceCollection().BuildServiceProvider());
        var viewModel = new CalendarPanelViewModel(navigationService, appointmentService, session);
        viewModel.SelectedAppointment = viewModel.Appointments[0];

        // WHEN
        viewModel.DeleteAppointmentCommand.Execute(null);

        // THEN
        Assert.Equal("Löschen bestätigen", viewModel.DeleteButtonText);
        Assert.Single(appointmentService.ViewAppointments());
        Assert.False(session.HasUnsavedChanges);

        // WHEN
        viewModel.DeleteAppointmentCommand.Execute(null);

        // THEN
        Assert.Equal("Löschen", viewModel.DeleteButtonText);
        Assert.Empty(appointmentService.ViewAppointments());
        Assert.True(session.HasUnsavedChanges);
    }

    [Fact]
    public void GivenPatient_PatientFieldsFromPatient_PopulatesFormFields()
    {
        // GIVEN
        Patient patient = new("Ada", "Lovelace", "AL1");
        patient.SetSalutation("Frau");
        patient.SetDateOfBirth(new DateOnly(1815, 12, 10));
        patient.SetAddress("Imaginary Road", "42B", "12345", "London", "UK", "near the analytical engine");
        patient.SetEmail("ada@example.com");
        patient.SetPhoneNumber("+123");
        patient.SetICD10Diagnosis("F12");
        patient.SetInsuranceStatus(PatientInsuranceStatus.Kostenerstattung);
        patient.IsActive = false;

        // WHEN
        PatientFields fields = PatientFields.FromPatient(patient);

        // THEN
        Assert.Equal("AL1", fields.PatientID);
        Assert.Equal("Frau", fields.Salutation);
        Assert.Equal("Ada", fields.FirstName);
        Assert.Equal("Lovelace", fields.LastName);
        Assert.Equal(new DateTime(1815, 12, 10), fields.DateOfBirth);
        Assert.Equal("Imaginary Road", fields.Street);
        Assert.Equal("42B", fields.HouseNumber);
        Assert.Equal("12345", fields.PostalCode);
        Assert.Equal("London", fields.Place);
        Assert.Equal("UK", fields.Country);
        Assert.Equal("near the analytical engine", fields.AdditionalInfo);
        Assert.Equal("ada@example.com", fields.Email);
        Assert.Equal("+123", fields.PhoneNumber);
        Assert.Equal("F12", fields.Icd10Diagnosis);
        Assert.Equal("Kostenerstattung", fields.InsuranceStatus);
        Assert.False(fields.IsActive);
    }
}
