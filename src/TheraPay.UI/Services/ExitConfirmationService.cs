using System;
using TheraPay.Domain;
using TheraPay.UI.Navigation;
using TheraPay.UI.State;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI.Services;

public sealed class ExitConfirmationService
{
    private readonly NavigationStore _navigationStore;
    private readonly NavigationService _navigationService;
    private readonly ProjectSession _session;
    private readonly ProjectPersistenceService _projectPersistence;

    private object? _previousViewModel;

    public bool IsPromptOpen { get; private set; }
    public event EventHandler? CloseApproved;

    public ExitConfirmationService(
        NavigationStore navigationStore,
        NavigationService navigationService,
        ProjectSession session,
        ProjectPersistenceService projectPersistence)
    {
        _navigationStore = navigationStore;
        _navigationService = navigationService;
        _session = session;
        _projectPersistence = projectPersistence;
    }

    public bool TryStartCloseFlow()
    {
        if (!_session.HasUnsavedChanges)
        {
            return false;
        }

        if (IsPromptOpen)
        {
            return true;
        }

        _previousViewModel = _navigationStore.CurrentViewModel;
        IsPromptOpen = true;
        _navigationService.NavigateTo<ExitConfirmViewModel>();
        return true;
    }

    public void CancelClose()
    {
        if (_previousViewModel is not null)
        {
            _navigationStore.Navigate(_previousViewModel);
        }

        ResetFlow();
    }

    public void CloseWithoutSaving()
    {
        ResetFlow();
        CloseApproved?.Invoke(this, EventArgs.Empty);
    }

    public Result SaveAndClose()
    {
        Result result = _projectPersistence.SaveProject();
        if (!result.Ok)
        {
            return result;
        }

        ResetFlow();
        CloseApproved?.Invoke(this, EventArgs.Empty);
        return result;
    }

    private void ResetFlow()
    {
        IsPromptOpen = false;
        _previousViewModel = null;
    }
}
