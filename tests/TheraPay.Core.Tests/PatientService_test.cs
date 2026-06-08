using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Core.Tests;

public class PatientService_test
{

    [Fact]
    public void GivenPatientRepository_CreatePatientService_PatientServiceHasRepository()
    {
        // GIVEN
        InMemoryPatientRepository repository = new InMemoryPatientRepository();

        // WHEN
        PatientService service = new PatientService(repository);

        // THEN
        Assert.Equal(repository.GetAll(), service.ViewPatients());
    }

    [Fact]
    public void GivenPatientRepositoryWithPatients_AddPatient_GetAllReturnsPatients()
    {
        // GIVEN
        InMemoryPatientRepository repository = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        PatientService service = new PatientService(repository);

        // // WHEN
        var result = service.AddPatient("Friedrich", "Gauss", "FG7");
        IReadOnlyList<Patient> patients = service.ViewPatients();

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(3, patients.Count);
        Assert.Equal("Friedrich", patients[2].FirstName);
        Assert.Equal("Gauss", patients[2].LastName);
        Assert.Equal("FG7", patients[2].ID);
    }

    [Fact]
    public void GivenPatientRepositoryWithPatients_AddPatient1_ResultIsNotOk()
    {
        // GIVEN
        PatientService service = TestData.getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients();

        // WHEN
        var patient = TestData.Patient1();
        var result = service.AddPatient(patient.FirstName, patient.LastName, patient.ID);

        // THEN
        Assert.False(result.Ok);
    }

    [Fact]
    public void GivenCompletePatientFormData_AddPatient_AllFieldsAreAdded()
    {
        // GIVEN
        InMemoryPatientRepository repository = new InMemoryPatientRepository();
        PatientService service = new PatientService(repository);

        // WHEN
        var result = service.AddPatient(
            "  Ada  ",
            "  Lovelace  ",
            "  AL1  ",
            "Imaginary Road",
            "42B",
            "12345",
            "London",
            "UK",
            "near the analytical engine",
            "my@email.com",
            "+123",
            "F12",
            "Kostenerstattung",
            false,
            "Frau",
            new DateOnly(1815, 12, 10));

        Patient patient = repository.GetById("AL1");

        // THEN
        Assert.True(result.Ok);
        Assert.Equal("Ada", patient.FirstName);
        Assert.Equal("Lovelace", patient.LastName);
        Assert.Equal("AL1", patient.ID);
        Assert.Equal("Frau", patient.Salutation);
        Assert.Equal(new DateOnly(1815, 12, 10), patient.DateOfBirth);
        Assert.NotNull(patient.Address);
        Assert.Equal("Imaginary Road", patient.Address.Street);
        Assert.Equal("42B", patient.Address.HouseNumber);
        Assert.Equal("12345", patient.Address.PostalCode);
        Assert.Equal("London", patient.Address.City);
        Assert.Equal("UK", patient.Address.Country);
        Assert.Equal("near the analytical engine", patient.Address.Additional);
        Assert.Equal("my@email.com", patient.Email);
        Assert.Equal("+123", patient.PhoneNumber);
        Assert.Equal("F12", patient.ICD10Diagnosis);
        Assert.Equal(PatientInsuranceStatus.Kostenerstattung, patient.InsuranceStatus);
        Assert.False(patient.IsActive);
    }

    [Fact]
    public void GivenEmptyOptionalFields_CheckPatientData_ResultIsOk()
    {
        // GIVEN
        PatientService service = new PatientService(new InMemoryPatientRepository());

        // WHEN
        Result result = service.CheckPatientData("P1", "", "", "", "");

        // THEN
        Assert.True(result.Ok);
    }

    [Fact]
    public void GivenMissingPatientId_CheckPatientData_ResultIsNotOk()
    {
        // GIVEN
        PatientService service = new PatientService(new InMemoryPatientRepository());

        // WHEN
        Result result = service.CheckPatientData("", "", "", "", "");

        // THEN
        Assert.False(result.Ok);
    }

    [Fact]
    public void GivenExistingPatientId_CheckPatientData_ResultIsNotOk()
    {
        // GIVEN
        PatientService service = TestData.getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients();
        Patient existingPatient = service.ViewPatients()[0];

        // WHEN
        Result result = service.CheckPatientData(existingPatient.ID, "", "", "", "");

        // THEN
        Assert.False(result.Ok);
    }

    [Theory]
    [InlineData("not-an-email", "", "", "")]
    [InlineData("", "123 456", "", "")]
    [InlineData("", "", "1234", "")]
    [InlineData("", "", "", "free diagnosis text")]
    public void GivenInvalidOptionalField_CheckPatientData_ResultIsNotOk(
        string email,
        string phoneNumber,
        string postalCode,
        string diagnosis)
    {
        // GIVEN
        PatientService service = new PatientService(new InMemoryPatientRepository());

        // WHEN
        Result result = service.CheckPatientData("P1", email, phoneNumber, postalCode, diagnosis);

        // THEN
        Assert.False(result.Ok);
    }

    [Fact]
    public void GivenExistingPatient_UpdatePatient_AllFieldsAreUpdated()
    {
        // GIVEN
        InMemoryPatientRepository repository = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        PatientService service = new PatientService(repository);

        // WHEN
        Result result = service.UpdatePatient(
            "L5R",
            "  Grace  ",
            "  Hopper  ",
            "Compiler Street",
            "7",
            "54321",
            "Arlington",
            "USA",
            "rear entrance",
            "grace@example.com",
            "+987",
            "A12",
            "GKV",
            false,
            "Divers",
            new DateOnly(1906, 12, 9));

        Patient patient = repository.GetById("L5R");

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(2, repository.Count());
        Assert.Equal("Grace", patient.FirstName);
        Assert.Equal("Hopper", patient.LastName);
        Assert.Equal("L5R", patient.ID);
        Assert.Equal("Divers", patient.Salutation);
        Assert.Equal(new DateOnly(1906, 12, 9), patient.DateOfBirth);
        Assert.NotNull(patient.Address);
        Assert.Equal("Compiler Street", patient.Address.Street);
        Assert.Equal("7", patient.Address.HouseNumber);
        Assert.Equal("54321", patient.Address.PostalCode);
        Assert.Equal("Arlington", patient.Address.City);
        Assert.Equal("USA", patient.Address.Country);
        Assert.Equal("rear entrance", patient.Address.Additional);
        Assert.Equal("grace@example.com", patient.Email);
        Assert.Equal("+987", patient.PhoneNumber);
        Assert.Equal("A12", patient.ICD10Diagnosis);
        Assert.Equal(PatientInsuranceStatus.GKV, patient.InsuranceStatus);
        Assert.False(patient.IsActive);
    }

    [Fact]
    public void GivenUnknownPatient_UpdatePatient_ResultIsNotOk()
    {
        // GIVEN
        PatientService service = TestData.getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients();

        // WHEN
        Result result = service.UpdatePatient(
            "unknown",
            "Grace",
            "Hopper",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Privat",
            true);

        // THEN
        Assert.False(result.Ok);
    }

}
