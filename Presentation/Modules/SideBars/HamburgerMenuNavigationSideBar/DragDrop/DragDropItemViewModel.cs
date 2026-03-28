using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Prism;
using Prism.Mvvm;
using Prism.Unity;

using Aksl.Toolkit.UI;
using Aksl.Modules.HamburgerMenuNavigationSideBar.Views;
using Aksl.Views;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class DragDropItemViewModel : BindableBase
    {
        #region Members
        private DragDropItem _dragDropItem;
        #endregion

        #region Constructors
        public DragDropItemViewModel(DragDropItem dragDropItem)
        {
            _dragDropItem = dragDropItem;

            X = _dragDropItem.X;
            Y = _dragDropItem.Y;
            Width = _dragDropItem.Width;
            Height = _dragDropItem.Height;
        }
        #endregion

        #region Properties
        private double _x;
        public double X
        {
            get => _x;
            set => SetProperty<double>(ref _x, value);
        }

        private double _y;
        public double Y
        {
            get => _y;
            set => SetProperty<double>(ref _y, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty<double>(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty<double>(ref _height, value);
        }

        public string ViewName => _dragDropItem.ViewName;

        private Type _viewElementType = default;
        public Type ViewElementType
        {
            get
            {
                if (_viewElementType is null)
                {
                    string viewTypeAssemblyQualifiedName = _dragDropItem.ViewName;
                    _viewElementType = Type.GetType(viewTypeAssemblyQualifiedName);
                }

                return _viewElementType;
            }
        }

        private DependencyObject _viewElement = default;
        public DependencyObject ViewElement
        {
            get
            {
                if (_viewElement is null)
                {
                    if (ViewElementType is not null)
                    {
                        // viewElemen = Activator.CreateInstance(viewType) as DependencyObject;
                        _viewElement = (PrismApplication.Current as PrismApplicationBase).Container.Resolve(ViewElementType) as DependencyObject;
                    }
                }

                return _viewElement;
            }
            set
            {
                SetProperty<DependencyObject>(ref _viewElement, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    if (!_isSelected)
                    {
                        IsDown = false; 
                        IsDragging = false;
                        StartPoint = new Point(0, 0);
                        OriginalElement = null;
                    }
                }
            }
        }

        private bool _isDown = false;
        public bool IsDown
        {
            get => _isDown;
            set => SetProperty<bool>(ref _isDown, value);
        }

        public bool IsDragging { get; set; }

        public Point StartPoint { get; set; }

        public UIElement OriginalElement { get; set; }

        public CircleAdorner OverlayElement { get; set; }
        #endregion

        #region Events
        private event EventHandler<MouseEventArgs> _mouseMove;
        public event EventHandler<MouseEventArgs> MouseMove
        {
            add { _mouseMove += value; }
            remove { _mouseMove -= value; }
        }
        #endregion

        #region PreviewMouseLeftButtonDown Event
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DragDropItemView dragDropItemView)
            {
                VisualTreeFinder visualTreeFinder = new();
                var nodeView = visualTreeFinder.FindVisualChild<XNodeView>(dragDropItemView);
                if(nodeView is null)
                {
                    return;
                }

                IsDown = true;
                OriginalElement = e.Source as UIElement;
                StartPoint = e.GetPosition(OriginalElement);

                OriginalElement.CaptureMouse();

                IsSelected = true;

                e.Handled = true;
            }
        }
        #endregion

        #region MouseMove Event
        public void ExecuteMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is DragDropItemView dragDropItemView)
            {
                if (IsSelected && IsDown)
                {
                    VisualTreeFinder visualTreeFinder = new();
                    System.Windows.Controls.Canvas mainCanvas = visualTreeFinder.FindVisualParent<System.Windows.Controls.Canvas>(dragDropItemView);

                    if (!this.IsDragging)
                    {
                        DragStarted();
                    }

                    if (this.IsDragging)
                    {
                        DragMoved();
                    }

                    void DragStarted()
                    {
                        this.IsDragging = true;
                    }

                    void DragMoved()
                    {
                        var mainCanvasPoint = Mouse.GetPosition(mainCanvas);

                        this.X = mainCanvasPoint.X - this.StartPoint.X;
                        this.Y = mainCanvasPoint.Y - this.StartPoint.Y;

                        //Debug.Print($"X:{this.X} Y:{this.Y}");
                        // 一旦靠近Left或Top边缘就停止移动
                        if (this.X <= 0)
                        {
                            this.X = 0;
                        }
                        if (this.X >= mainCanvas.ActualWidth - this.Width)
                        {
                            this.X = mainCanvas.ActualWidth - this.Width;
                        }

                        if (this.Y <= 0)
                        {
                            this.Y = 0;
                        }
                        if (this.Y >= mainCanvas.ActualHeight - this.Height)
                        {
                            this.Y = mainCanvas.ActualHeight - this.Height;
                        }
                    }
                }

                _mouseMove?.Invoke(sender, e);
            }
        }
        #endregion

        #region MouseLeftButtonUp Event
        public void ExecuteMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (IsSelected && this.IsDown)
            {
                DragFinished();
            }

            void DragFinished(bool cancelled = false)
            {
                Debug.Print($"DragDropItemView:MouseLeftButtonUp");

                OriginalElement.ReleaseMouseCapture();
                Mouse.Capture(null);

                this.IsDragging = false;
                this.IsDown = false;
            }

            e.Handled = true;
        }
        #endregion
    }
}
