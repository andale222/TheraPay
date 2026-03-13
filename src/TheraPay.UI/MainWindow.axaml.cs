using Avalonia.Controls;
using System;
using TheraPay.UI.Services;
using TheraPay.UI.ViewModels;

namespace TheraPay.UI;

public partial class MainWindow : Window
{
    private readonly ExitConfirmationService _exitConfirmation;

    public MainWindow(MainWindowViewModel vm, ExitConfirmationService exitConfirmation)
    {
        InitializeComponent();
        DataContext = vm;
        _exitConfirmation = exitConfirmation;
        _exitConfirmation.CloseApproved += OnCloseApproved;
    }

    private bool _allowClose = false;

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_allowClose) return;

        if (_exitConfirmation.TryStartCloseFlow())
        {
            e.Cancel = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _exitConfirmation.CloseApproved -= OnCloseApproved;
        base.OnClosed(e);
    }

    private void OnCloseApproved(object? sender, EventArgs e)
    {
        _allowClose = true;
        Close();
    }
}
