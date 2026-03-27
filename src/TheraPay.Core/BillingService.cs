namespace TheraPay.Core;

using System.Reflection.Metadata;
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

    private List<InvoiceAppointmentData> GetAppointmentDataListFromAppointmentIdList(List<Guid> appointmentIds)
    {
        var appointmentDataList = new List<InvoiceAppointmentData>();
        foreach (var aptmt in appointmentIds)
        {
            bool exists = _appointmentRepository.GetAll().Any(p => p.Id == aptmt);
            if (exists)
            {
                appointmentDataList.Add(InvoiceAppointmentData.FromAppointmentData(
                    _appointmentRepository.GetById(aptmt)));
            }
        }
        return appointmentDataList;
    }
    public Result AddInvoiceForPatientAndAppointments(string patientId, List<Guid> appointmentIds, PracticeData practiceData)
    {
        // TODO: Die eingegebenen Daten sollten geprüft werden und bei fehlerhaften oder nichtexistenten Daten eine Fehlermeldung zurückgegeben werden, da hier user-input verarbeitet wird
        try
        {
            var patientData = InvoicePatientData.FromPatientData(_patientRepository.GetById(patientId));
            var appointmentDataList = GetAppointmentDataListFromAppointmentIdList(appointmentIds);
            var practiceDataRecord = PracticeDataRecord.FromPracticeData(practiceData);
            var invoice = new Invoice(patientData, appointmentDataList, practiceDataRecord);
            _invoiceRepository.Add(invoice);

            return new Result(true);
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
}