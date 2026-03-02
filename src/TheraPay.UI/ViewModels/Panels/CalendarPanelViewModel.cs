using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TheraPay.Domain;
using TheraPay.Core;              // Patient, PatientService (ggf. Namespace anpassen)
using TheraPay.UI.Navigation;     // NavigationService
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.ViewModels.Panels;

public sealed class CalendarPanelViewModel : ViewModelBase
{
    private readonly NavigationService _nav;

public CalendarPanelViewModel(NavigationService nav)
    {
        _nav = nav;
    }
}