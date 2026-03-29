namespace TheraPay.Domain;

public static class TestData
{

    public static Patient Patient1( ) => new Patient("A", "J", "L5R");
    public static Patient Patient2( ) => new Patient("second", "patient", "NR2");
    public static Appointment Appointment1( ) => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), Patient1().ID);
    public static Appointment Appointment2( ) => new Appointment(new DateTime(2026, 1, 8, 15, 0, 0), Patient2().ID);
    public static Appointment Appointment1_2( ) => new Appointment(new DateTime(2026, 2, 1, 14, 0, 0), Patient1().ID);

    public static PracticeData PracticeData1() => new PracticeData()
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
    public static Invoice CreateInvoice()
    {
        Patient patient = Patient1( );
        var patientData = InvoicePatientData.FromPatientData(patient);
        Appointment appointment1 = Appointment1();
        Appointment appointment2 = Appointment1_2();
        var appointmentData = new List<InvoiceAppointmentData>()
        {
            InvoiceAppointmentData.FromAppointmentData(appointment1),
            InvoiceAppointmentData.FromAppointmentData(appointment2),
        };
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(PracticeData1());

        // WHEN
        var invoice = new Invoice(patientData, appointmentData, practiceDataRecord);

        return invoice;
    }
}