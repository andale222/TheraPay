namespace TheraPay.Core;

using TheraPay.Domain;

public class BillingService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;

    public BillingService(IInvoiceRepository repository, IAppointmentRepository appointmentRepository, IPatientRepository patientRepository)
    {
        _invoiceRepository = repository;
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
    }

    private List<Appointment> GetExistingAppointmentsFromAppointmentIds(List<Guid> appointmentIds)
    {
        var appointmentList = new List<Appointment>();
        foreach (var aptmt in appointmentIds)
        {
            bool exists = _appointmentRepository.GetAll().Any(p => p.Id == aptmt);
            if (exists)
            {
                appointmentList.Add(_appointmentRepository.GetById(aptmt));
            }
        }
        return appointmentList;
    }
    private Result FilterOutBilledAppointments(List<Appointment> appointmentList)
    {
        int nBilledAppointmentsRemoved = 0;
        for (int i = appointmentList.Count - 1; i >= 0; i--)
        {
            bool isBilled = appointmentList[i].Status == AppointmentStatus.Billed;
            if (isBilled)
            {
                appointmentList.RemoveAt(i);
                nBilledAppointmentsRemoved++;
            }
        }

        if (nBilledAppointmentsRemoved == 0)
            return new Result(true, "");

        return new Result(true, "" + nBilledAppointmentsRemoved + " billed appointments were removed. ");
    }
    private List<InvoiceAppointmentData> GetAppointmentDataListFromAppointments(List<Appointment> appointments)
    {
        var appointmentDataList = new List<InvoiceAppointmentData>();
        foreach (var aptmt in appointments)
        {
            appointmentDataList.Add(InvoiceAppointmentData.FromAppointmentData(aptmt));
        }
        return appointmentDataList;
    }
    public Result AddInvoiceForPatientAndAppointments(string patientId, List<Guid> appointmentIds, PracticeData practiceData)
    {
        try
        {
            // store warnings and info here, which the user should be informed about but does not impact the functioning
            string info = "";

            // Check PatientId exists
            if (_patientRepository.GetAll().Any(p => p.ID == patientId) == false)
                return new Result(false, $"Patient with ID {patientId} not found.");

            var patientData = InvoicePatientData.FromPatientData(_patientRepository.GetById(patientId));

            var distinctAppointmentIds = appointmentIds.Distinct().ToList();
            if (appointmentIds.Count != distinctAppointmentIds.Count)
                info += "" + (appointmentIds.Count - distinctAppointmentIds.Count) + " double appointment entry was removed.";

            var appointmentList = GetExistingAppointmentsFromAppointmentIds(distinctAppointmentIds);
            if (appointmentList.Count < distinctAppointmentIds.Count)
                info += "" + (distinctAppointmentIds.Count - appointmentList.Count) + " appointment ids were not found. ";

            var filterResult = FilterOutBilledAppointments(appointmentList);
            info += filterResult.Error;

            var appointmentDataList = GetAppointmentDataListFromAppointments(appointmentList);
            // Check appointmentIds is not empty
            if (appointmentDataList.Count == 0)
                return new Result(false, "No valid appointments found for the provided appointment IDs.");

            // Check appointments use patientId 
            bool allAppointmentsMatchPatientId = appointmentDataList.All(x => x.PatientId == patientData.Id);
            if (!allAppointmentsMatchPatientId)
                return new Result(false, "Mismatch between patient Id and appointments patient Id.");


            var practiceDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
            var invoice = new Invoice(patientData, appointmentDataList, practiceDataRecord);


            var result = _invoiceRepository.Add(invoice);
            if (!result.Ok)
                return new Result(false, result.Error);

            // TODO: setze Termine als billed? oder eher, wenn die invoice geissued wird?

            return new Result(true, info);
        }
        catch (Exception ex)
        {
            return new Result(false, ex.Message);
        }
    }
    public IReadOnlyList<Invoice> ViewInvoices()
    {
        return _invoiceRepository.GetAll();
    }

    public Result IssueInvoice(Invoice invoice, DateTime issueDate, PracticeData practiceData)
    {
        /* This function should do the following:
        - check invoice state
        - create new invoice number
        - issue the invoice with the new invoice number
        - set appointments to billed
        - print the invoice as pdf???
        */
        if (invoice == null)
            return new Result(false, "Invoice not found.");

        // first only preview next serial
        var invoiceNumber = practiceData.InvoiceNumberState.PreviewNextSerial(issueDate);
        var practiceDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
        var result = invoice.Issue(practiceDataRecord, invoiceNumber);
        if (!result.Ok)
            return new Result(false, result.Error);

        // consume invoice number after successful issueing
        var invoiceNumberConsumed = practiceData.InvoiceNumberState.ConsumeNextSerial(issueDate);
        Console.WriteLine($"Preview Invoice Number: {invoiceNumber}");
        Console.WriteLine($"Consumed Invoice Number: {invoiceNumberConsumed}");

        foreach (var appointmentData in invoice.AppointmentDataList)
        {
            _appointmentRepository.GetById(appointmentData.AppointmentId).SetStatusToBilled();
        }

        return new Result(true, "" + invoiceNumber);
    }

}

