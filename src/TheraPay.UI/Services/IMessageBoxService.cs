using System.Threading.Tasks;

namespace TheraPay.UI.Services;

public interface IMessageBoxService
{
    Task ShowErrorAsync(string title, string message);
    Task ShowWarningAsync(string title, string message);
    Task<bool> ConfirmWarningAsync(string title, string message, string confirmText = "OK", string cancelText = "Abbrechen");
}
