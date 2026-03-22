namespace TheraPay.Domain.Tests;

public class Invoice_test
{
    public static InvoicePatientData CreatePatientData()
    {
        Patient patient = new Patient("A", "J", "L5R");
        var patientData = new InvoicePatientData()
        {
            Name = patient.FirstName + " " + patient.LastName,
            Id = patient.ID
        };
        return patientData;
    }
    public static List<InvoiceAppointmentData> CreateAppointmenttDataListWithTwoEntries()
    {
        Appointment appointment1 = new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), "L5R");
        Appointment appointment2 = new Appointment(new DateTime(2026, 2, 1, 14, 0, 0), "L5R");
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment1.Id.ToString("D"),
                Date = appointment1.Date,
                PatientId = appointment1.PatientID
            },
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment2.Id.ToString("D"),
                Date = appointment2.Date,
                PatientId = appointment1.PatientID
            },
        };

        return appointmentData;
    }
    public static PracticeData CreatePracticeData()
    {
        return new PracticeData()
        {
            Name = "Physio Praxis 7",
            Street = "Beispielweg",
            HouseNumber = "12",
            PostalCode = "12345",
            City = "Musterstadt",
            Country = "Deutschland",
            AddressAdditional = "Hinterhaus",
            TaxIdentificationNumber = "987654321",
            IBAN = "DE77 1234 5678 9012 3456 78",
            BLZ = "77665544",
            BankName = "Musterbank",
            Subject = "Rechnung Therapie",
            DefaultPaymentTermDays = 30,
            InvoiceNumberState = InvoiceNumberState.Rehydrate(DateTime.Today.Year, 1200, 1)
        };
    }

    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.Name, invoice.PatientData.Name);
        Assert.Equal(patientData.Id, invoice.PatientData.Id);
    }
    [Fact]
    public void GivenInvoicesAppointmentData_CreateInvoice_InvoiceHasCorrectAppointmentData()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(appointmentData[0].AppointmentId, invoice.AppointmentDataList[0].AppointmentId);
        Assert.Equal(appointmentData[0].Date, invoice.AppointmentDataList[0].Date);
    }
    [Fact]
    public void GivenInvoiceData_CreateInvoice_InvoiceIsDraft()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        // THEN
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
    [Fact]
    public void GivenEmptyInvoice_IssueInvoice_StatusIsIssuedAndDatesAreSet()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        // WHEN
        invoice.Issue();

        // THEN
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(DateTime.Today, invoice.IssueDate);
        Assert.Equal(DateTime.Today.AddDays(practiceData.DefaultPaymentTermDays), invoice.DueDate);
        Assert.Equal($"{DateTime.Today:yyyyMM}-1201", invoice.InvoiceNumber);
        Assert.Equal(2, practiceData.InvoiceNumberState.NextIssueNumber);
    }

    [Fact]
    public void GivenInvoiceWithPatientAndAppointmentData_CreateInvoice_InvoiceHasBothCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        //     // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.Name, invoice.PatientData.Name);
        Assert.Equal(patientData.Id, invoice.PatientData.Id);
        Assert.Equal(appointmentData[0].AppointmentId, invoice.AppointmentDataList[0].AppointmentId);
        Assert.Equal(appointmentData[0].Date, invoice.AppointmentDataList[0].Date);
    }


    [Fact]
    public void GivenInvoiceWithDifferingFirstPatientId_CreateInvoice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var practiceData = CreatePracticeData();
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Appointment appointment = new Appointment(date, "OR5");
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment.Id.ToString("D"),
                Date = appointment.Date,
                PatientId = appointment.PatientID
            }
        };

        // WHEN THEN
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceData));
    }
    [Fact]
    public void GivenInvoiceWithDifferingSecondPatientId_CreateInvoice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var practiceData = CreatePracticeData();
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Appointment appointment = new Appointment(date, "L5R");
        Appointment appointment2 = new Appointment(date, "OR5");
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment.Id.ToString("D"),
                Date = appointment.Date,
                PatientId = appointment.PatientID
            },
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment2.Id.ToString("D"),
                Date = appointment2.Date,
                PatientId = appointment2.PatientID
            }
        };

        // WHEN THEN
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceData));
    }
    [Fact]
    public void GivenInvoiceData_CreateInvoiceWithTheSameAppointmentTwice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var practiceData = CreatePracticeData();
        DateTime date = new DateTime(2026, 1, 1, 14, 0, 0);
        Appointment appointment = new Appointment(date, "L5R");
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment.Id.ToString("D"),
                Date = appointment.Date,
                PatientId = appointment.PatientID
            },
            new InvoiceAppointmentData()
            {
                AppointmentId = appointment.Id.ToString("D"),
                Date = appointment.Date,
                PatientId = appointment.PatientID
            }
        };

        // WHEN THEN
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceData));
    }

    [Fact]
    public void GivenPracticeData_CreateInvoice_PracticeDataRecordIsExtractedAndAvailable()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = CreatePracticeData();

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceData);

        // THEN
        Assert.Equal(practiceData.Name, invoice.PracticeDataRecord.Name);
        Assert.Equal(practiceData.Street, invoice.PracticeDataRecord.Address.Street);
        Assert.Equal(practiceData.HouseNumber, invoice.PracticeDataRecord.Address.HouseNumber);
        Assert.Equal(practiceData.PostalCode, invoice.PracticeDataRecord.Address.PostalCode);
        Assert.Equal(practiceData.City, invoice.PracticeDataRecord.Address.City);
        Assert.Equal(practiceData.Country, invoice.PracticeDataRecord.Address.Country);
        Assert.Equal(practiceData.AddressAdditional, invoice.PracticeDataRecord.Address.Additional);
        Assert.Equal(practiceData.TaxIdentificationNumber, invoice.PracticeDataRecord.TaxNumber);
        Assert.Equal(practiceData.IBAN, invoice.PracticeDataRecord.PaymentDetails.IBAN);
        Assert.Equal(practiceData.BLZ, invoice.PracticeDataRecord.PaymentDetails.BLZ);
        Assert.Equal(practiceData.BankName, invoice.PracticeDataRecord.PaymentDetails.BankName);
        Assert.Equal(practiceData.Subject, invoice.PracticeDataRecord.PaymentDetails.Subject);
        Assert.Equal(practiceData.DefaultPaymentTermDays, invoice.PracticeDataRecord.DefaultPaymentTermDays);
    }

    [Fact]
    public void GivenTwoInvoicesFromSamePracticeData_IssueBoth_InvoiceNumbersAreUniqueAndSequential()
    {
        // GIVEN
        var practiceData = CreatePracticeData();
        var invoice1 = new Invoice(CreatePatientData(), CreateAppointmenttDataListWithTwoEntries(), practiceData);
        var invoice2 = new Invoice(CreatePatientData(), CreateAppointmenttDataListWithTwoEntries(), practiceData);

        // WHEN
        invoice1.Issue();
        invoice2.Issue();

        // THEN
        Assert.Equal($"{DateTime.Today:yyyyMM}-1201", invoice1.InvoiceNumber);
        Assert.Equal($"{DateTime.Today:yyyyMM}-1202", invoice2.InvoiceNumber);
        Assert.Equal(3, practiceData.InvoiceNumberState.NextIssueNumber);
    }

    [Fact]
    public void GivenInvoiceNumberStateFromDifferentYear_IssueInvoice_StateResetsToCurrentYear()
    {
        // GIVEN
        var practiceData = CreatePracticeData();
        practiceData.InvoiceNumberState = InvoiceNumberState.Rehydrate(
            DateTime.Today.Year - 1,
            1500,
            999);

        var invoice = new Invoice(CreatePatientData(), CreateAppointmenttDataListWithTwoEntries(), practiceData);

        // WHEN
        invoice.Issue();

        // THEN
        Assert.StartsWith($"{DateTime.Today:yyyyMM}-", invoice.InvoiceNumber);
        Assert.Equal(DateTime.Today.Year, practiceData.InvoiceNumberState.Year);
        Assert.Equal(2, practiceData.InvoiceNumberState.NextIssueNumber);

        var serial = int.Parse(invoice.InvoiceNumber.Split('-')[1]);
        Assert.InRange(serial, 1001, 9000);
    }
}
