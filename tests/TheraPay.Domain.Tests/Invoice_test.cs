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
        Assert.Equal(2.468m, invoice.TotalAmount); // TODO: add actual correct amount!
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
