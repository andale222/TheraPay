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

    [Fact]
    public void GivenInvoicesPatientData_CreateInvoice_InvoiceHasCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

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

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

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

        // WHEN
        var invoice = new Invoice(patientData, appointmentData);

        // THEN
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }
    [Fact]
    public void GivenEmptyInvoice_IssueInvoice_StatusIsIssuedAndDatesAreSet()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();
        var invoice = new Invoice(patientData, appointmentData);

        // WHEN
        invoice.Issue();

        // THEN
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal(DateTime.Today, invoice.IssueDate);
        Assert.Equal(DateTime.Today.AddDays(14), invoice.DueDate); // TODO: add function to set Due date properly
    }

    [Fact]
    public void GivenInvoiceWithPatientAndAppointmentData_CreateInvoice_InvoiceHasBothCorrectValues()
    {
        // GIVEN
        var patientData = CreatePatientData();
        var appointmentData = CreateAppointmenttDataListWithTwoEntries();

        //     // WHEN
        var invoice = new Invoice(patientData, appointmentData);

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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData));
    }
    [Fact]
    public void GivenInvoiceWithDifferingSecondPatientId_CreateInvoice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData));
    }
    [Fact]
    public void GivenInvoiceData_CreateInvoiceWithTheSameAppointmentTwice_InvoiceRaisesException()
    {
        // GIVEN
        var patientData = CreatePatientData();
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
        Assert.Throws<ArgumentException>(() => new Invoice(patientData, appointmentData));
    }
}