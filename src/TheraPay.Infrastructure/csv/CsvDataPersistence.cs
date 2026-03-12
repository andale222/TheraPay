using TheraPay.Core;

namespace TheraPay.Infrastructure.csv;

public sealed class CsvDataPersistence : IDataPersistence
{
    private readonly CsvPatientStore _patientStore;
    private readonly CsvAppointmentStore _appointmentStore;

    public CsvDataPersistence(CsvPatientStore patientStore, CsvAppointmentStore appointmentStore)
    {
        _patientStore = patientStore;
        _appointmentStore = appointmentStore;
    }

    public void LoadInto(IPatientRepository patientRepository, IAppointmentRepository appointmentRepository)
    {
        foreach (var patient in _patientStore.LoadAll())
        {
            patientRepository.Add(patient);
        }

        foreach (var appointment in _appointmentStore.LoadAll())
        {
            appointmentRepository.Add(appointment);
        }
    }

    public void SaveFrom(IPatientRepository patientRepository, IAppointmentRepository appointmentRepository)
    {
        _patientStore.SaveAll(patientRepository.GetAll());
        _appointmentStore.SaveAll(appointmentRepository.GetAll());
    }
}