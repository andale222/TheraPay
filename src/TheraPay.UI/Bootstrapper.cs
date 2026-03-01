using Microsoft.Extensions.DependencyInjection;
using TheraPay.Core; // Services / Interfaces
using TheraPay.UI.ViewModels;
using TheraPay.UI.ViewModels.Panels;
using TheraPay.UI.Views;
using TheraPay.UI.Navigation;

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
        services.AddSingleton<InMemoryPatientRepository, InMemoryPatientRepository>();
        // services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();

        // Services (Use-Cases) -> Singleton ok im MVP
        services.AddSingleton<PatientService>();
        // services.AddSingleton<AppointmentService>();

        // ViewModels -> oft Transient (pro View eine frische Instanz)
        services.AddTransient<PatientsViewModel>();
        services.AddTransient<PatientPanelViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<MainWindowViewModel>();

        // Views/Windows -> DI kann sie bauen (Ctor Injection)
        services.AddTransient<HomeView>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}