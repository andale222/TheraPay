using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core; // Services / Interfaces
using TheraPay.UI.ViewModels;
using TheraPay.UI.ViewModels.Panels;
using TheraPay.UI.Views;
using TheraPay.UI.Navigation;
using TheraPay.UI.Services;
using TheraPay.UI.State;

namespace TheraPay.UI;

public static class Bootstrapper
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        // Navigation
        services.AddSingleton<NavigationStore>();
        services.AddSingleton<NavigationService>();

        // Repositories (State) -> meist Singleton
        services.AddSingleton<InMemoryPatientRepository>();
        services.AddSingleton<InMemoryAppointmentRepository>();
        services.AddSingleton<InMemoryInvoiceRepository>();
        services.AddSingleton<IPatientRepository>(sp => sp.GetRequiredService<InMemoryPatientRepository>());
        services.AddSingleton<IAppointmentRepository>(sp => sp.GetRequiredService<InMemoryAppointmentRepository>());
        services.AddSingleton<IInvoiceRepository>(sp => sp.GetRequiredService<InMemoryInvoiceRepository>());

        // Project state + persistence orchestration
        services.AddSingleton<ProjectSession>();
        services.AddSingleton<ProjectPersistenceService>();
        services.AddSingleton<ExitConfirmationService>();

        // Services (Use-Cases) -> Singleton ok im MVP
        services.AddSingleton<PatientService>();
        services.AddSingleton<AppointmentService>();
        services.AddSingleton<BillingService>();

        // ViewModels -> oft Transient (pro View eine frische Instanz)
        services.AddTransient<LoadFilesViewModel>();
        services.AddTransient<AppointmentEditViewModel>();
        services.AddTransient<PatientsViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ExitConfirmViewModel>();
        services.AddTransient<InvoiceCreationViewModel>();
        services.AddTransient<MainWindowViewModel>();
        // Panels
        services.AddTransient<PatientPanelViewModel>();
        services.AddTransient<CalendarPanelViewModel>();

        // Views/Windows -> DI kann sie bauen (Ctor Injection)
        services.AddTransient<LoadFilesView>();
        services.AddTransient<HomeView>();
        services.AddTransient<ExitConfirmView>();
        services.AddTransient<MainWindow>();
        // services.AddTransient<AppointmentEditView>();

        return services.BuildServiceProvider();
    }
}
