using System;

using Prism.Services.Dialogs;

namespace Aksl.Dialogs.Services
{
    public static class DialogServiceExtensions
    {
        public static void Alert(this IDialogService dialogService, string message, string title, string okText, string windowName = null, Action<IDialogResult> callBack = null) =>
            dialogService.ShowDialog(nameof(Views.ConfirmView), new DialogParameters { { "IsConfirm", false }, { "Message", message }, { "Title", title }, { "OkText", okText }, }, callBack, windowName);

        public static void Confirm(this IDialogService dialogService, string message, string title, string okText, string cancelText, string windowName = null, Action<IDialogResult> callBack = null) =>
            dialogService.ShowDialog(nameof(Views.ConfirmView), new DialogParameters { { "IsConfirm", true }, { "Message", message }, { "Title", title }, { "OkText", "确定" }, { "CancelText", "取消" } }, callBack, windowName);
    }
}
