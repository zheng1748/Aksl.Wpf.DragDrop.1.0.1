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

        public SimpleCircleAdorner OverlayElement { get; set; }
        #endregion

        #region Events
        private event EventHandler<MouseEventArgs> _mouseMove;
        public event EventHandler<MouseEventArgs> MouseMove
        {
            add { _mouseMove += value; }
            remove { _mouseMove -= value; }
        }
        #endregion

        #region MouseLeftButtonDown Event
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Print($"DragDropItemView:MouseLeftButtonDown");

            System.Windows.Controls.Canvas canvas = null;
            FindCanvas();

            void FindCanvas()
            {
                if (sender is FrameworkElement element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var itemsControl = visualTreeFinder.FindVisualParent<ItemsControl>(element);

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(itemsControl);
                    canvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;
                }
            }

            IsDown = true;
            // StartPoint = e.GetPosition(canvas);
            OriginalElement = e.Source as UIElement;
            StartPoint = e.GetPosition(OriginalElement);
            //canvas?.CaptureMouse();
            OriginalElement.CaptureMouse();
            IsSelected = true;

            e.Handled = true;
        }
        #endregion

        #region MouseMove Event
        public void ExecutePreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Debug.Print($"DragDropItemView:MouseMove");

            if (IsSelected && IsDown)
            {
                System.Windows.Controls.Canvas canvas;

                if (sender is FrameworkElement element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var itemsControl = visualTreeFinder.FindVisualParent<ItemsControl>(element);

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(itemsControl);
                    canvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;

                    if (!this.IsDragging)
                    {
                        DragStarted();
                    }

                    if (this.IsDragging)
                    {
                        DragMoved();
                    }
                }

                void DragStarted()
                {
                    this.IsDragging = true;
                }

                void DragMoved()
                {
                    var currentPosition = Mouse.GetPosition(canvas);

                    this.X = currentPosition.X - this.StartPoint.X;
                    this.Y = currentPosition.Y - this.StartPoint.Y;

                    //Debug.Print($"X:{this.X} Y:{this.Y}");
                    // 一旦靠近Left或Top边缘就停止移动
                    if (this.X <= 0)
                    {
                        this.X = 0;
                    }
                    if (this.X >= canvas.ActualWidth-this.Width)
                    {
                        this.X = canvas.ActualWidth - this.Width;
                    }

                    if (this.Y <= 0)
                    {
                        this.Y = 0;
                    }
                    if (this.Y >= canvas.ActualHeight- this.Height)
                    {
                        this.Y = canvas.ActualHeight - this.Height;
                    }
                }
            }

            _mouseMove?.Invoke(sender, e);
        }
        #endregion

        #region MouseLeftButtonUp Event
        public void ExecutePreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Debug.Print($"DragDropItemView:MouseLeftButtonUp");

            if (IsSelected && this.IsDown)
            {
                DragFinished();
            }

            e.Handled = true;

            void DragFinished(bool cancelled = false)
            {
                OriginalElement.ReleaseMouseCapture();
                Mouse.Capture(null);

                //if (this.IsDragging)
                //{
                //    this.X = this.X;
                //    this.Y = this.Y;
                //}
                this.IsDragging = false;
                this.IsDown = false;
            }
        }
        #endregion
    }
}
