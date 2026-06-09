using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace TheraPay.UI.Services;

public sealed class MessageBoxService : IMessageBoxService
{
    public Task ShowErrorAsync(string title, string message)
    {
        return ShowAsync(title, message, Brushes.IndianRed, "OK", null);
    }

    public Task ShowWarningAsync(string title, string message)
    {
        return ShowAsync(title, message, Brushes.DarkOrange, "OK", null);
    }

    public Task<bool> ConfirmWarningAsync(string title, string message, string confirmText = "OK", string cancelText = "Abbrechen")
    {
        return ShowAsync(title, message, Brushes.DarkOrange, confirmText, cancelText);
    }

    private static async Task<bool> ShowAsync(
        string title,
        string message,
        IBrush titleBrush,
        string confirmText,
        string? cancelText)
    {
        var owner = TryGetMainWindow();
        if (owner is null)
        {
            return false;
        }

        var dialog = new Window
        {
            Title = title,
            Width = 460,
            MinWidth = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = titleBrush,
            TextWrapping = TextWrapping.Wrap
        };

        var messageBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        if (!string.IsNullOrWhiteSpace(cancelText))
        {
            var cancelButton = BuildDialogButton(cancelText);
            cancelButton.Click += (_, _) => dialog.Close(false);
            buttonPanel.Children.Add(cancelButton);
        }

        var confirmButton = BuildDialogButton(confirmText);
        confirmButton.Click += (_, _) => dialog.Close(true);
        buttonPanel.Children.Add(confirmButton);

        dialog.Content = new Border
        {
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    titleBlock,
                    messageBlock,
                    buttonPanel
                }
            }
        };

        return await dialog.ShowDialog<bool>(owner);
    }

    private static Button BuildDialogButton(string text)
    {
        return new Button
        {
            Content = text,
            MinWidth = 100,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
    }

    private static Window? TryGetMainWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}
