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
        Result checkResult = CheckPatientData(id, "", "", "", "");
        if (checkResult.Ok == false)
            return checkResult;

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
        bool isActive,
        string salutation = "",
        DateOnly? dateOfBirth = null)
    {
        Result checkResult = CheckPatientData(id, email, phoneNumber, postalCode, diagnosis);
        if (checkResult.Ok == false)
            return checkResult;

        Patient patient = new Patient(firstName.Trim(), lastName.Trim(), id.Trim());

        Result applyResult = ApplyPatientFormData(
            patient,
            firstName,
            lastName,
            street,
            houseNumber,
            postalCode,
            place,
            country,
            additionalInfo,
            email,
            phoneNumber,
            diagnosis,
            insuranceStatus,
            isActive,
            salutation,
            dateOfBirth);

        if (applyResult.Ok == false)
            return applyResult;

        return _repository.Add(patient);
    }

    private Result CheckOtherPatientData(
        string email,
        string phoneNumber,
        string postalCode,
        string diagnosis)
    {
        string resultMsg = "";
        bool resultStatus = true;

        Result result;
        if ((result = CheckEmail(email)).Ok == false)
        {
            resultStatus = false;
            resultMsg += "\n" + result.Error;
        }
        if ((result = CheckPhoneNumber(phoneNumber)).Ok == false)
        {
            resultStatus = false;
            resultMsg += "\n" + result.Error;
        }
        if ((result = CheckPostalCode(postalCode)).Ok == false)
        {
            resultStatus = false;
            resultMsg += "\n" + result.Error;
        }
        if ((result = CheckDiagnosis(diagnosis)).Ok == false)
        {
            resultStatus = false;
            resultMsg += "\n" + result.Error;
        }

        return new Result(resultStatus, resultMsg);
    }
    public Result CheckPatientData(
        string id,
        string email,
        string phoneNumber,
        string postalCode,
        string diagnosis)
    {
        string resultMsg = "";
        bool resultStatus = true;
        string trimmedId = id.Trim();
        if (string.IsNullOrWhiteSpace(trimmedId))
        {
            resultStatus = false;
            resultMsg += "Patienten-ID ist erforderlich.";
        }
        if (_repository.GetIndexById(trimmedId) >= 0)
        {
            resultStatus = false;
            resultMsg += $"Patienten-ID '{trimmedId}' ist bereits vorhanden.";
        }

        Result result = CheckOtherPatientData(email, phoneNumber, postalCode, diagnosis);

        return new Result(resultStatus && result.Ok, resultMsg+result.Error);
    }

    public Result CheckPatientUpdateData(
        string id,
        string email,
        string phoneNumber,
        string postalCode,
        string diagnosis)
    {
        string resultMsg = "";
        bool resultStatus = true;
        string trimmedId = id.Trim();
        if (string.IsNullOrWhiteSpace(trimmedId))
        {
            resultStatus = false;
            resultMsg += "Patienten-ID ist erforderlich.";
        }
        else if (_repository.GetIndexById(trimmedId) < 0)
        {
            resultStatus = false;
            resultMsg += $"Patienten-ID '{trimmedId}' wurde nicht gefunden.";
        }

        Result result = CheckOtherPatientData(email, phoneNumber, postalCode, diagnosis);

        return new Result(resultStatus && result.Ok, resultMsg+result.Error);
    }

    public Patient? FindPatientById(string id)
    {
        string trimmedId = id.Trim();
        int index = _repository.GetIndexById(trimmedId);
        return index >= 0 ? _repository.GetByIndex(index) : null;
    }

    public Result UpdatePatient(
        string id,
        string firstName,
        string lastName,
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
        bool isActive,
        string salutation = "",
        DateOnly? dateOfBirth = null)
    {
        string trimmedId = id.Trim();
        Result checkResult = CheckPatientUpdateData(trimmedId, email, phoneNumber, postalCode, diagnosis);
        if (checkResult.Ok == false)
            return checkResult;

        Patient? patient = FindPatientById(trimmedId);
        if (patient is null)
            return new Result(false, $"Patienten-ID '{trimmedId}' wurde nicht gefunden.");

        return ApplyPatientFormData(
            patient,
            firstName,
            lastName,
            street,
            houseNumber,
            postalCode,
            place,
            country,
            additionalInfo,
            email,
            phoneNumber,
            diagnosis,
            insuranceStatus,
            isActive,
            salutation,
            dateOfBirth);
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

    private Result ApplyPatientFormData(
        Patient patient,
        string firstName,
        string lastName,
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
        bool isActive,
        string salutation,
        DateOnly? dateOfBirth)
    {
        try
        {
            patient.SetName(firstName, lastName);
            patient.SetSalutation(salutation);
            patient.SetDateOfBirth(dateOfBirth);

            Result addressResult = ModifyAddressFromFormData(
                patient,
                street,
                houseNumber,
                postalCode,
                place,
                country,
                additionalInfo);
            if (addressResult.Ok == false)
                return addressResult;

            Result contactResult = ModifyContactData(patient, email, phoneNumber);
            if (contactResult.Ok == false)
                return contactResult;

            Result diagnosisResult = ModifyDiagnosis(patient, diagnosis);
            if (diagnosisResult.Ok == false)
                return diagnosisResult;

            SetInsuranceStatus(patient, insuranceStatus);
            SetActivityStatus(patient, isActive);

            return new Result(true);
        }
        catch (ArgumentException ex)
        {
            return new Result(false, ex.Message);
        }
    }

    private Result ModifyAddressFromFormData(
        Patient patient,
        string street,
        string houseNumber,
        string postalCode,
        string place,
        string? country,
        string? additionalInfo)
    {
        bool hasRequiredAddressData =
            string.IsNullOrWhiteSpace(street) == false ||
            string.IsNullOrWhiteSpace(houseNumber) == false ||
            string.IsNullOrWhiteSpace(postalCode) == false ||
            string.IsNullOrWhiteSpace(place) == false;
        bool hasOptionalAddressData =
            string.IsNullOrWhiteSpace(country) == false ||
            string.IsNullOrWhiteSpace(additionalInfo) == false;

        if (hasRequiredAddressData == false)
        {
            if (hasOptionalAddressData)
                return new Result(false, "Adressdaten sind unvollständig: Straße, Hausnr., PLZ und Ort sind erforderlich.");

            patient.ClearAddress();
            return new Result(true);
        }

        if (string.IsNullOrWhiteSpace(street) ||
            string.IsNullOrWhiteSpace(houseNumber) ||
            string.IsNullOrWhiteSpace(postalCode) ||
            string.IsNullOrWhiteSpace(place))
        {
            return new Result(false, "Adressdaten sind unvollständig: Straße, Hausnr., PLZ und Ort sind erforderlich.");
        }

        ModifyAddress(patient, street, houseNumber, postalCode, place, country, additionalInfo);
        return new Result(true);
    }

    public Result CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || Patient.EmailRegex.IsMatch(email.Trim()))
            return new Result(true);
        else
            return new Result(false, "Email must be in a valid format like user@domain.com");
    }

    public Result CheckPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || Patient.PhoneNumberRegex.IsMatch(phoneNumber.Trim()))
            return new Result(true);
        else
            return new Result(false, "Phone number may only contain digits and an optional leading '+'.");
    }

    public Result CheckPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode) || Address.PostalCodeRegex.IsMatch(postalCode.Trim()))
            return new Result(true);
        else
            return new Result(false, "Postleitzahl muss leer sein oder aus 5 Ziffern bestehen.");
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
        if (string.IsNullOrWhiteSpace(diagnosis) || Patient.Icd10DiagnosisRegex.IsMatch(diagnosis.Trim()))
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
