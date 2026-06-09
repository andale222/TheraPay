namespace TheraPay.Domain.Tests;

public class Invoice_test
{
    public static InvoicePatientData CreatePatientData()
    {
        return InvoicePatientData.FromPatientData(TestData.Patient1());
    }
    public static List<InvoiceAppointmentData> CreateAppointmenttDataListWithTwoEntries()
    {
        Appointment appointment1 = TestData.Appointment1();
        Appointment appointment2 = TestData.Appointment1_2();
        var billingNumber = BillingNumberCatalog.FindByIdentifier("801a")!;
        appointment1.AssignBillingNumber(billingNumber);
        appointment2.AssignBillingNumber(billingNumber);
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            InvoiceAppointmentData.FromAppointmentData(appointment1),
            InvoiceAppointmentData.FromAppointmentData(appointment2),
        };

        return appointmentData;
    }

    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(patientData.Name, invoice.PatientData.Name);
        Assert.Equal(patientData.Id, invoice.PatientData.Id);
        Assert.Equal(Invoice.DefaultSubject, invoice.Subject);
    }

    [Fact]
    public void GivenPatientWithAddress_FromPatientData_PatientAddressIsCopied()
    {
        // GIVEN
        var patient = new Patient("Ada", "Lovelace", "AL1");
        patient.SetAddress("Imaginary Road", "42B", "12345", "London", "UK", "near the analytical engine");

        // WHEN
        var patientData = InvoicePatientData.FromPatientData(patient);

        // THEN
        Assert.Equal("Ada Lovelace", patientData.Name);
        Assert.Equal("AL1", patientData.Id);
        Assert.Equal("Imaginary Road", patientData.Street);
        Assert.Equal("42B", patientData.HouseNumber);
        Assert.Equal("12345", patientData.PostalCode);
        Assert.Equal("London", patientData.City);
        Assert.Equal("UK", patientData.Country);
        Assert.Equal("near the analytical engine", patientData.AddressAdditional);
        Assert.Equal("Imaginary Road 42B", patientData.StreetAndHouseNumber);
        Assert.Equal("12345 London", patientData.PostalCodeAndCity);
    }
    [Fact]
    public void GivenInvoicesAppointmentData_CreateInvoice_InvoiceHasCorrectAppointmentData()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // THEN
        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(appointmentData[0].AppointmentId, invoice.AppointmentDataList[0].AppointmentId);
        Assert.Equal(appointmentData[0].Date, invoice.AppointmentDataList[0].Date);
        Assert.Equal(appointmentData[0].BillingNumbers, invoice.AppointmentDataList[0].BillingNumbers);
    }
    [Fact]
    public void GivenInvoicesAppointmentData_CreateInvoice_InvoiceHasCorrectTotalAmount()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // THEN
        Assert.Equal(appointmentData.Sum(appointment => appointment.TotalAmount), invoice.TotalAmount);
    }

    [Fact]
    public void GivenInvoiceDraft_SetDraftDetails_InvoiceDateAndPaymentTermAreStored()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);
        var issueDate = new DateTime(2026, 3, 13);

        // WHEN
        var result = invoice.SetDraftDetails(issueDate, 21, "Bitte beachten.", "Individuelle Therapie");

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(issueDate, invoice.IssueDate);
        Assert.Equal(issueDate.AddDays(21), invoice.DueDate);
        Assert.Equal(21, invoice.PracticeDataRecord.DefaultPaymentTermDays);
        Assert.Equal("Bitte beachten.", invoice.AdditionalText);
        Assert.Equal("Individuelle Therapie", invoice.Subject);
    }
    [Fact]
    public void GivenInvoiceData_CreateInvoice_InvoiceIsDraft()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // THEN
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
    [Fact]
    public void GivenEmptyInvoice_IssueInvoice_StatusIsIssuedAndDatesAreSet()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = TestData.PracticeData1();
        var invoice = new Invoice(patientData, appointmentData, PracticeDataRecord.FromPracticeData(practiceData));
        var invoiceNr = $"{DateTime.Today:yyyyMM}-1201";
        // WHEN
        var result = invoice.Issue(PracticeDataRecord.FromPracticeData(practiceData), invoiceNr);

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(DateTime.Today, invoice.IssueDate);
        Assert.Equal(DateTime.Today.AddDays(practiceData.DefaultPaymentTermDays), invoice.DueDate);
        Assert.Equal(invoiceNr, invoice.InvoiceNumber);
    }

    [Fact]
    public void GivenInvoiceAndExplicitIssueDate_IssueInvoice_UsesExplicitIssueDateAndPaymentTerm()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceData = TestData.PracticeData1();
        practiceData.DefaultPaymentTermDays = 21;
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);
        var issueDate = new DateTime(2026, 3, 13);
        var invoiceNr = $"{issueDate:yyyyMM}-1201";

        // WHEN
        var result = invoice.Issue(practiceDataRecord, invoiceNr, issueDate);

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(issueDate, invoice.IssueDate);
        Assert.Equal(issueDate.AddDays(21), invoice.DueDate);
        Assert.Equal(invoiceNr, invoice.InvoiceNumber);
    }

    [Fact]
    public void GivenDraftInvoice_SetPostIssueStatus_ReturnsFalseAndKeepsDraft()
    {
        // GIVEN
        var invoice = new Invoice(
            CreatePatientData(),
            CreateAppointmenttDataListWithTwoEntries(),
            PracticeDataRecord.FromPracticeData(TestData.PracticeData1()));

        // WHEN
        var result = invoice.SetPostIssueStatus(InvoiceStatus.Payed, DateTime.Today);

        // THEN
        Assert.False(result.Ok);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }

    [Fact]
    public void GivenIssuedInvoice_SetPostIssueStatusToPayed_StatusIsPayed()
    {
        // GIVEN
        var practiceData = TestData.PracticeData1();
        var issueDate = new DateTime(2026, 3, 13);
        var invoice = new Invoice(
            CreatePatientData(),
            CreateAppointmenttDataListWithTwoEntries(),
            PracticeDataRecord.FromPracticeData(practiceData));
        var issueResult = invoice.Issue(
            PracticeDataRecord.FromPracticeData(practiceData),
            $"{issueDate:yyyyMM}-1201",
            issueDate);
        Assert.True(issueResult.Ok);

        // WHEN
        var result = invoice.SetPostIssueStatus(InvoiceStatus.Payed, issueDate.AddDays(1));

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(InvoiceStatus.Payed, invoice.Status);
    }

    [Fact]
    public void GivenPastDueInvoice_SetPostIssueStatusToIssued_StatusIsOverdue()
    {
        // GIVEN
        var practiceData = TestData.PracticeData1();
        practiceData.DefaultPaymentTermDays = 1;
        var issueDate = new DateTime(2026, 3, 13);
        var invoice = new Invoice(
            CreatePatientData(),
            CreateAppointmenttDataListWithTwoEntries(),
            PracticeDataRecord.FromPracticeData(practiceData));
        var issueResult = invoice.Issue(
            PracticeDataRecord.FromPracticeData(practiceData),
            $"{issueDate:yyyyMM}-1201",
            issueDate);
        Assert.True(issueResult.Ok);

        // WHEN
        var result = invoice.SetPostIssueStatus(InvoiceStatus.Issued, issueDate.AddDays(2));

        // THEN
        Assert.True(result.Ok);
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void GivenInvalidInvoiceNumberFormat_IssueInvoice_ReturnsFalseAndKeepsDraft()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // WHEN
        var result = invoice.Issue(practiceDataRecord, "202613-1201");

        // THEN
        Assert.False(result.Ok);
        Assert.Equal("Error in invoice number or invoice number format.",result.Error);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal(string.Empty, invoice.InvoiceNumber);
    }

    [Fact]
    public void GivenInvoiceWithPatientAndAppointmentData_CreateInvoice_InvoiceHasBothCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

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
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());
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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceDataRecord));
    }
    [Fact]
    public void GivenInvoiceWithDifferingSecondPatientId_CreateInvoice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());
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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceDataRecord));
    }
    [Fact]
    public void GivenInvoiceData_CreateInvoiceWithTheSameAppointmentTwice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());
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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData, practiceDataRecord));
    }

    [Fact]
    public void GivenPracticeData_CreateInvoice_PracticeDataRecordIsExtractedAndAvailable()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(TestData.PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        // THEN
        Assert.Equal(practiceDataRecord, invoice.PracticeDataRecord);
    }
}
