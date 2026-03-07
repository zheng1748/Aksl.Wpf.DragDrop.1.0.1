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

namespace Aksl.ViewModels
{
    public class XNodeViewModel : BindableBase
    {
        #region Members
        #endregion

        #region Constructors
        public XNodeViewModel()
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

        private Visibility _borderVisible = Visibility.Collapsed;
        public Visibility BorderVisible
        {
            get => _borderVisible;
            set => SetProperty<Visibility>(ref _borderVisible, value);
        }

        private bool _isFocused = false;
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if (SetProperty<bool>(ref _isFocused, value))
                {
                    BorderVisible = _isFocused ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private ContextMenu _popupMenu;
        public ContextMenu PopupMenu
        {
            get => _popupMenu;
            set => SetProperty(ref _popupMenu, value);
        }
        #endregion

        #region Events
        private event EventHandler<MouseButtonEventArgs> _mouseRightButtonDown;
        public event EventHandler<MouseButtonEventArgs> MouseRightButtonDown
        {
            add { _mouseRightButtonDown += value; }
            remove { _mouseRightButtonDown -= value; }
        }

        private event EventHandler<MouseButtonEventArgs> _mouseLeftButtonDown;
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown
        {
            add { _mouseLeftButtonDown += value; }
            remove { _mouseLeftButtonDown -= value; }
        }

        private event EventHandler<MouseEventArgs> _mouseMove;
        public event EventHandler<MouseEventArgs> MouseMove
        {
            add { _mouseMove += value; }
            remove { _mouseMove -= value; }
        }

        private event EventHandler<MouseButtonEventArgs> _mouseLeftButtonUp;
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonUp
        {
            add { _mouseLeftButtonUp += value; }
            remove { _mouseLeftButtonUp -= value; }
        }

        private event EventHandler<MouseButtonEventArgs> _outputNodePreviewMouseLeftButtonDown;
        public event EventHandler<MouseButtonEventArgs> OutputNodePreviewMouseLeftButtonDown
        {
            add { _outputNodePreviewMouseLeftButtonDown += value; }
            remove { _outputNodePreviewMouseLeftButtonDown -= value; }
        }
        #endregion

        #region Loaded Event
        //public void Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (sender is System.Windows.Controls.UserControl uc)
        //    {
        //        VisualTreeFinder visualTreeFinder = new();
        //        var borders = visualTreeFinder.FindVisualChilds<Border>(uc);
        //        var outputNodeBorder= borders.FirstOrDefault(b=>b.Name== "OutputNode");

        //        uc.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ExecuteMouseLeftButtonDown),false);
        //        outputNodeBorder.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ExecuteOutputNodeMouseLeftButtonDown), false);
        //    }
        //}
        #endregion

        #region MouseRightButtonDown Event
        public void ExecuteMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseRightButtonDown?.Invoke(sender, e);
        }
       #endregion

        #region MouseLeftButtonDown Event
        public void ExecutePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsFocused = true;
        }
        #endregion

        #region ExecuteMouseEnter Event
        public void ExecuteMouseEnter(object sender, MouseEventArgs e)
        {
          // Debug.Print($"XNodeViewModel:MouseEnter");

            if (sender is UserControl uc)
            {
                if (!IsFocused)
                {
                    Storyboard polylineMarginOutStoryboard = (Storyboard)uc.Resources["PolylineMarginOutStoryboard"];
                    Storyboard polylineColorOutStoryboard = (Storyboard)uc.Resources["PolylineColorOutStoryboard"];

                    polylineMarginOutStoryboard.Begin();
                    polylineColorOutStoryboard.Begin();
                }
            }

            if (BorderVisible == Visibility.Collapsed)
            {
                BorderVisible = Visibility.Visible;
            }
           
            e.Handled= true;
        }
        #endregion

        #region ExecuteMouseLeave Event
        public void ExecuteMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsFocused)
            {
                BorderVisible = Visibility.Collapsed;
            }

            e.Handled = true;
        }
        #endregion

        #region OutputNode MouseLeftButtonDown Event
        public void ExecuteOutputNodePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _outputNodePreviewMouseLeftButtonDown?.Invoke(sender, e);
        }
        #endregion
    }
}
