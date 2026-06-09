namespace TheraPay.Core;

public interface IDataPersistence
{
    void LoadInto(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository);

    void SaveFrom(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository);
}
