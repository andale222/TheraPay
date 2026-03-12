namespace TheraPay.Core;

public interface IDataPersistence
{
    void LoadInto(IPatientRepository patientRepository); // , IAppointmentRepository appointmentRepository

    void SaveFrom(IPatientRepository patientRepository); // , IAppointmentRepository appointmentRepository
}