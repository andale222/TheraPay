namespace TheraPay.Core;

using TheraPay.Domain;

public class PatientService
{
    private readonly IPatientRepository _repository;
    private const PatientInsuranceStatus DefaultInsuranceStatus = PatientInsuranceStatus.Privat;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public Result AddPatient(string firstName, string lastName, string id)
    {
        Patient patient = new Patient(firstName, lastName, id);
        Result result = _repository.Add(patient);
        return result;
    }

    public Result AddPatient(
        string firstName,
        string lastName,
        string id,
        string street,
        string houseNumber,
        string postalCode,
        string place,
        string? country,
        string? additionalInfo,
        string email,
        string phoneNumber,
        string diagnosis,
        string insuranceStatus,
        bool isActive)
    {
        Patient patient = new Patient(firstName.Trim(), lastName.Trim(), id.Trim());

        ModifyAddress(patient, street, houseNumber, postalCode, place, country, additionalInfo);
        ModifyContactData(patient, email, phoneNumber);
        ModifyDiagnosis(patient, diagnosis);
        SetInsuranceStatus(patient, insuranceStatus);
        SetActivityStatus(patient, isActive);

        return _repository.Add(patient);
    }

    public void ModifyAddress(
        Patient patient,
        string street,
        string houseNumber,
        string postalCode,
        string place,
        string? country = null,
        string? additionalInfo = null)
    {
        patient.SetAddress(new Address(street, houseNumber, postalCode, place, country, additionalInfo));
    }

    public Result CheckEmail(string email)
    {
        if (Patient.EmailRegex.IsMatch(email.Trim()))
            return new Result(true);
        else
            return new Result(false, "Email must be in a valid format like user@domain.com");
    }
    public Result CheckPhoneNumber(string phoneNumber)
    {
        if (Patient.PhoneNumberRegex.IsMatch(phoneNumber.Trim()))
            return new Result(true);
        else
            return new Result(false, "Phone number may only contain digits and an optional leading '+'.");
    }
    public Result ModifyContactData(Patient patient, string email, string phoneNumber)
    {
        Result result;
        if (!(result = CheckEmail(email)).Ok) return result;
        if (!(result = CheckPhoneNumber(phoneNumber)).Ok) return result;

        patient.SetEmail(email);
        patient.SetPhoneNumber(phoneNumber);

        return new Result(true);
    }

    public Result CheckDiagnosis(string diagnosis)
    {
        if (Patient.Icd10DiagnosisRegex.IsMatch(diagnosis.Trim()))
            return new Result(true);
        else
            return new Result(false, "Diagnosis must be a valid ICD-10-CM code.");
    }
    public Result ModifyDiagnosis(Patient patient, string diagnosis)
    {
        Result result;
        if (!(result = CheckDiagnosis(diagnosis)).Ok) return result;

        patient.SetICD10Diagnosis(diagnosis);

        return new Result(true);
    }

    public void SetActivityStatus(Patient patient, bool isActive)
    {
        patient.IsActive = isActive;
    }

    public void SetActivityStatus(Patient patient, string activityStatus)
    {
        string normalizedStatus = activityStatus.Trim().ToLowerInvariant();
        patient.IsActive = normalizedStatus is not ("inaktiv" or "inactive" or "false" or "0");
    }

    public void SetInsuranceStatus(Patient patient, string insuranceStatus)
    {
        if (Enum.TryParse<PatientInsuranceStatus>(insuranceStatus.Trim(), ignoreCase: true, out var parsedStatus))
            SetInsuranceStatus(patient, parsedStatus);
        else
            SetInsuranceStatus(patient, DefaultInsuranceStatus);
    }

    public void SetInsuranceStatus(Patient patient, PatientInsuranceStatus insuranceStatus)
    {
        patient.SetInsuranceStatus(insuranceStatus);
    }

    public IReadOnlyList<Patient> ViewPatients()
    {
        return _repository.GetAll();
    }
}
