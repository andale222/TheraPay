using TheraPay.UI.Services;

namespace TheraPay.UI.ViewModels;

public sealed class ExitConfirmViewModel : ViewModelBase
{
    private readonly ExitConfirmationService _exitConfirmation;

    public RelayCommand SaveAndExitCommand { get; }
    public RelayCommand ExitWithoutSavingCommand { get; }
    public RelayCommand CancelCommand { get; }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ExitConfirmViewModel(ExitConfirmationService exitConfirmation)
    {
        _exitConfirmation = exitConfirmation;
        SaveAndExitCommand = new RelayCommand(SaveAndExit);
        ExitWithoutSavingCommand = new RelayCommand(() => _exitConfirmation.CloseWithoutSaving());
        CancelCommand = new RelayCommand(() => _exitConfirmation.CancelClose());
    }

    private void SaveAndExit()
    {
        var result = _exitConfirmation.SaveAndClose();
        if (!result.Ok)
        {
            StatusMessage = result.Error ?? "Speichern fehlgeschlagen.";
        }
    }
}
