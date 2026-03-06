using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Aksl.Toolkit.UI;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Aksl.ViewModels
{
    public class EditXNodeViewXNodeViewModel : BindableBase, IDialogAware
    {
        #region Members
        #endregion

        #region Constructors
        public EditXNodeViewXNodeViewModel()
        {
        }
        #endregion

        #region Properties
        private Brush _headerBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8"));
        public Brush HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => SetProperty<Brush>(ref _headerBackgroundColor, value);
        }

        private Brush _headerForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public Brush HeaderForegroundColor
        {
            get => _headerForegroundColor;
            set => SetProperty<Brush>(ref _headerForegroundColor, value);
        }

        private Brush _contentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public Brush ContentBackgroundColor
        {
            get => _contentBackgroundColor;
            set => SetProperty<Brush>(ref _contentBackgroundColor, value);
        }

        private object _content = "节点信息";
        public object Content
        {
            get => _content;
            set => SetProperty<object>(ref _content, value);
        }

        private double _lineWidth = 3d;
        public double LineWidth
        {
            get => _lineWidth;
            set => SetProperty<double>(ref _lineWidth, value);
        }

        private double _shrink = 3d;
        public double Shrink
        {
            get => _shrink;
            set => SetProperty<double>(ref _shrink, value);
        }
        #endregion

        #region Dialog Properties
        private string _title = "Notification";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _okText = "OK";
        public string OkText
        {
            get => _okText;
            set => SetProperty(ref _okText, value);
        }

        private string _cancelText = "Cancel";
        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }
        #endregion

        #region IDialogAware
        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("Title") ?? "Notification";
            OkText = parameters.GetValue<string>("OkText") ?? "OK";
            CancelText = parameters.GetValue<string>("CancelText") ?? "Cancel";
        }
        #endregion
    }
}
