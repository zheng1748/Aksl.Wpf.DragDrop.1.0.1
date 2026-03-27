using System;
using System.Threading.Tasks;

using Prism.Services.Dialogs;

namespace Aksl.Toolkit.Services
{
    public interface IDialogViewService
    {
        Task AlertAsync(string message, string title = null, string okText = "Ok", Action<IDialogResult> callBack = null,string windowName = null);

        Task ConfirmAsync(string message, string title = null, string okText = "Ok", string cancelText = "Cancel", Action<IDialogResult> callBack = null,string windowName = null);
    }

    public class DialogViewService : IDialogViewService
    {
        #region Members
        private readonly IDialogService _dialogService;
        #endregion

        #region Constructors
        public DialogViewService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
        #endregion

        public Task AlertAsync(string message, string title = null, string okText = "Ok", Action<IDialogResult> callBack = null,string windowName = null)
        {
            _dialogService.Alert(message: message, title: title, okText: okText, callBack: callBack, windowName: windowName);

            return Task.CompletedTask;
        }

        public Task ConfirmAsync(string message, string title = null, string okText = "Ok", string cancelText = "Cancel", Action<IDialogResult> callBack = null,string windowName = null)
        {
            _dialogService.Confirm(title: title, message: message, okText: okText, cancelText: cancelText, callBack: callBack, windowName: windowName);

            return Task.CompletedTask;
        }
    }

    public static class DialogExtensions
    {
        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title)
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message, title);
            }
        }

        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title, string okText = "Ok")
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message, title, okText: okText);
            }
        }
    }
}
