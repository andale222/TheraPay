using TheraPay.Core;
using TheraPay.Domain;

namespace TheraPay.Infrastructure.csv.Tests;

public class CsvDataPersistence_test
{
    [Fact]
    public void GivenCsvPatientStoreAndPatientRepository_LoadInto_FillsPatientRepository()
    {
        // Given
        var dataDirPatients = TestPaths.DataFile("testLoadPatients.csv");
        var dataDirAppointments = TestPaths.DataFile("testLoadAppointments.csv");
        var patientStore = new CsvPatientStore(dataDirPatients);
        var appointmentStore = new CsvAppointmentStore(dataDirAppointments);
        var dataPersistence = new CsvDataPersistence(patientStore, appointmentStore);
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();

        // When
        var loadedPatients = patientStore.LoadAll();
        var loadedAppointments = appointmentStore.LoadAll();
        dataPersistence.LoadInto(patientRepository, appointmentRepository);

        // Then patients lsit
        var patients = patientRepository.GetAll().ToList();
        Assert.NotEmpty(patients);
        Assert.Equal(loadedPatients.Count(), patients.Count);
        foreach (var loadedPatient in loadedPatients)
        {
            Assert.Contains(patients, p => p.ID == loadedPatient.ID);
        }

        // Then appointments list
        var appointments = appointmentRepository.GetAll().ToList();
        Assert.NotEmpty(appointments);
        Assert.Equal(loadedAppointments.Count(), appointments.Count);
        foreach (var loadedAppointment in loadedAppointments)
        {
            Assert.Contains(appointments, a =>
                a.Id == loadedAppointment.Id &&
                a.Date == loadedAppointment.Date &&
                a.PatientID == loadedAppointment.PatientID);
        }
    }

    [Fact]
    public void GivenCsvPatientStoreAndPatientRepository_SaveFrom_SavesPatientsToCsv()
    {
        // Given
        var dataDirPatients = TestPaths.DataFile("testSavePatients_DataPersistence.csv");
        var dataDirAppointments = TestPaths.DataFile("testSaveAppointments_DataPersistence.csv");
        var patientStore = new CsvPatientStore(dataDirPatients);
        var appointmentStore = new CsvAppointmentStore(dataDirAppointments);
        var dataPersistence = new CsvDataPersistence(patientStore, appointmentStore);
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patient1 = new Patient("John", "Doe", "1d3");
        var patient2 = new Patient("Jane", "Smith", "wed");
        patientRepository.Add(patient1);
        patientRepository.Add(patient2);
        var appointment1 = new Appointment(DateTime.Now, patient1.ID);
        var appointment2 = new Appointment(DateTime.Now.AddDays(1), patient2.ID);
        appointmentRepository.Add(appointment1);
        appointmentRepository.Add(appointment2);

        // When
        dataPersistence.SaveFrom(patientRepository, appointmentRepository);

        // Then
        var loadedPatients = patientStore.LoadAll();
        Assert.NotEmpty(loadedPatients);
        Assert.Equal(2, loadedPatients.Count());
        Assert.Contains(loadedPatients, p => p.ID == patient1.ID);
        Assert.Contains(loadedPatients, p => p.ID == patient2.ID);
        File.Delete(dataDirPatients);
        Assert.False(File.Exists(dataDirPatients));

        var loadedAppointments = appointmentStore.LoadAll();
        Assert.NotEmpty(loadedAppointments);
        Assert.Equal(2, loadedAppointments.Count());
        Assert.Contains(loadedAppointments, a => a.Id == appointment1.Id && a.Date == appointment1.Date && a.PatientID == appointment1.PatientID);
        Assert.Contains(loadedAppointments, a => a.Id == appointment2.Id && a.Date == appointment2.Date && a.PatientID == appointment2.PatientID);
        File.Delete(dataDirAppointments);
        Assert.False(File.Exists(dataDirAppointments));
    }   

}
