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
        var dataDirInvoices = TestPaths.DataFile("testLoadInvoices_missing.csv");
        var patientStore = new CsvPatientStore(dataDirPatients);
        var appointmentStore = new CsvAppointmentStore(dataDirAppointments);
        var invoiceStore = new CsvInvoiceStore(dataDirInvoices);
        var dataPersistence = new CsvDataPersistence(patientStore, appointmentStore, invoiceStore);
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var invoiceRepository = new InMemoryInvoiceRepository();

        // When
        var loadedPatients = patientStore.LoadAll();
        var loadedAppointments = appointmentStore.LoadAll();
        dataPersistence.LoadInto(patientRepository, appointmentRepository, invoiceRepository);

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

        Assert.Empty(invoiceRepository.GetAll());
    }

    [Fact]
    public void GivenCsvPatientStoreAndPatientRepository_SaveFrom_SavesPatientsToCsv()
    {
        // Given
        var dataDirPatients = TestPaths.DataFile("testSavePatients_DataPersistence.csv");
        var dataDirAppointments = TestPaths.DataFile("testSaveAppointments_DataPersistence.csv");
        var dataDirInvoices = TestPaths.DataFile("testSaveInvoices_DataPersistence.csv");
        var patientStore = new CsvPatientStore(dataDirPatients);
        var appointmentStore = new CsvAppointmentStore(dataDirAppointments);
        var invoiceStore = new CsvInvoiceStore(dataDirInvoices);
        var dataPersistence = new CsvDataPersistence(patientStore, appointmentStore, invoiceStore);
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var invoiceRepository = new InMemoryInvoiceRepository();
        var patient1 = new Patient("John", "Doe", "1d3");
        var patient2 = new Patient("Jane", "Smith", "wed");
        patientRepository.Add(patient1);
        patientRepository.Add(patient2);
        var appointment1 = new Appointment(DateTime.Now, patient1.ID);
        var appointment2 = new Appointment(DateTime.Now.AddDays(1), patient2.ID);
        appointmentRepository.Add(appointment1);
        appointmentRepository.Add(appointment2);
        var practiceData = new PracticeData
        {
            Street = "Testweg",
            HouseNumber = "1",
            PostalCode = "12345",
            City = "Teststadt",
            IBAN = "DE00",
            DefaultPaymentTermDays = 14
        };
        var invoice = new Invoice(
            InvoicePatientData.FromPatientData(patient1),
            [InvoiceAppointmentData.FromAppointmentData(appointment1)],
            PracticeDataRecord.FromPracticeData(practiceData));
        invoice.SetDraftDetails(new DateTime(2026, 1, 1), 14);
        invoiceRepository.Add(invoice);

        // When
        dataPersistence.SaveFrom(patientRepository, appointmentRepository, invoiceRepository);

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

        var loadedInvoices = invoiceStore.LoadAll();
        Assert.Single(loadedInvoices);
        Assert.Equal(invoice.Id, loadedInvoices[0].Id);
        Assert.Equal(invoice.Status, loadedInvoices[0].Status);
        File.Delete(dataDirInvoices);
        Assert.False(File.Exists(dataDirInvoices));
    }   

}
