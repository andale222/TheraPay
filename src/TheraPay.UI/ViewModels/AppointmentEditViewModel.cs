using System.Collections.ObjectModel;
using TheraPay.Domain;
using TheraPay.Core;
using TheraPay.UI.Navigation;

namespace TheraPay.UI.ViewModels;

public sealed class AppointmentEditViewModel : ViewModelBase
{
    private readonly NavigationService _nav;

public AppointmentEditViewModel(NavigationService nav)
    {
        _nav = nav;
    }
}