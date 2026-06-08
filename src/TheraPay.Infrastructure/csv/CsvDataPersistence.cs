using TheraPay.Core;

namespace TheraPay.Infrastructure.csv;

public sealed class CsvDataPersistence : IDataPersistence
{
    private readonly CsvPatientStore _patientStore;
    private readonly CsvAppointmentStore _appointmentStore;
    private readonly CsvInvoiceStore _invoiceStore;

    public CsvDataPersistence(CsvPatientStore patientStore, CsvAppointmentStore appointmentStore, CsvInvoiceStore invoiceStore)
    {
        _patientStore = patientStore;
        _appointmentStore = appointmentStore;
        _invoiceStore = invoiceStore;
    }

    public void LoadInto(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository)
    {
        foreach (var patient in _patientStore.LoadAll())
        {
            patientRepository.Add(patient);
        }

        foreach (var appointment in _appointmentStore.LoadAll())
        {
            appointmentRepository.Add(appointment);
        }

        foreach (var invoice in _invoiceStore.LoadAll())
        {
            invoiceRepository.Add(invoice);
        }
    }

    public void SaveFrom(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository)
    {
        _patientStore.SaveAll(patientRepository.GetAll());
        _appointmentStore.SaveAll(appointmentRepository.GetAll());
        _invoiceStore.SaveAll(invoiceRepository.GetAll());
    }
}
