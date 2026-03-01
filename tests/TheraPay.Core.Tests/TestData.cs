namespace TheraPay.Core.Tests;

using TheraPay.Core;
using TheraPay.Domain;

public static class TestData
{

    public static Patient Patient1( ) => new Patient("A", "J", "L5R");
    public static Patient Patient2( ) => new Patient("second", "patient", "NR2");


    public static InMemoryPatientRepository getInMemoryPatientRepositoryWithTwoPatients()
    {
        InMemoryPatientRepository repository = new InMemoryPatientRepository();
        repository.Add(Patient1());
        repository.Add(Patient2());
        return repository;
    }
    public static PatientService getPatientServiceWithInMemoryPatientRepositoryWithTwoPatients()
    {
        
        InMemoryPatientRepository repository = TestData.getInMemoryPatientRepositoryWithTwoPatients();
        return new PatientService(repository);
    }



    public static Appointment Appointment1( ) => new Appointment(new DateTime(2026, 1, 1, 14, 0, 0), Patient1().ID);
    public static Appointment Appointment2( ) => new Appointment(new DateTime(2026, 1, 8, 15, 0, 0), Patient2().ID);

    public static AppointmentService getAppointmentServiceWithInMemoryAppointmentRepositoryWithTwoAppointments()
    {
        InMemoryAppointmentRepository repository = new InMemoryAppointmentRepository();
        Appointment appointment1 = TestData.Appointment1();
        appointment1.SetDuration(60);
        Appointment appointment2 = TestData.Appointment2();
        appointment2.SetDuration(30);
        repository.Add(appointment1);
        repository.Add(appointment2);
        return new AppointmentService(repository);
    }
}