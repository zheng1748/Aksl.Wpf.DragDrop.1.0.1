using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Prism.Mvvm;

using Aksl.Toolkit.UI;
using System.Xml.Linq;
using Aksl.Views;
using Aksl.ViewModels;
using Aksl.Modules.HamburgerMenuNavigationSideBar.Views;
using Prism.Unity;
using Prism;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class DragDropViewModel : BindableBase
    {
        #region Members
        private DragDropItemViewModel _selectedDragDropItem;
        private readonly UIElement _canvas;
        private Point? _selectedRectangleStartPoint;
        #endregion

        #region Constructors
        public DragDropViewModel()
        {
            DragDropItems = new();

            AddSelectionRectangle();
            void AddSelectionRectangle()
            {
                Rectangle rectangle = new()
                {
                    Name = "SelectionRectangle",
                    Height = 0,
                    Width = 0,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeDashArray = new(){ 2d, 4d },
                    Fill = new SolidColorBrush(Color.FromScRgb(0x40, 0x00, 0xA0, 0xFF)),
                    Visibility = Visibility.Collapsed
                };
                DragDropItem dragDropItem = new() { X = 0, Y = 0, Width = 0, Height = 0 };
                DragDropItemViewModel dragDropItemViewModel = new(dragDropItem) { ViewElement = rectangle };
                AddPropertyChanged(dragDropItemViewModel);
                DragDropItems.Add(dragDropItemViewModel);
            }
        }
        #endregion

        #region Properties
        public ObservableCollection<DragDropItemViewModel> DragDropItems { get; }
        public DragDropItemViewModel PreviewSelectedDragDropItem { get; private set; }
        #endregion

        #region Drop Event
        public void ExecuteDrop(object sender, DragEventArgs e)
        {
            var menuItem = e.Data.GetData(typeof(Infrastructure.MenuItem)) as Infrastructure.MenuItem;

            var point = e.GetPosition((IInputElement)e.Source);

            DragDropItem dragDropItem = new() { X = point.X, Y = point.Y, Width = menuItem.Width, Height = menuItem.Height, ViewName = menuItem.ViewName };
            DragDropItemViewModel dragDropItemViewModel = new(dragDropItem);

            VisualTreeFinder visualTreeFinder = new();
            var childs = visualTreeFinder.FindLogicalChilds<System.Windows.DependencyObject>(dragDropItemViewModel.ViewElement);
            var nodeView = childs.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
            var nodeModel = nodeView.DataContext as XNodeViewModel;
            nodeModel.OutputNodeMouseLeftButtonDown += OutputNodeMouseLeftButtonDown;
            dragDropItemViewModel.MouseMove += DragDropItemMouseMove;
            AddPropertyChanged(dragDropItemViewModel);

            DragDropItems.Add(dragDropItemViewModel);

           var element= e.Source as ItemsControl;
            //VisualTreeFinder visualTreeFinder = new();
            //var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
            //var nodeView = childs.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
            //var nodeModel = nodeView.DataContext as XNodeViewModel;
        }

        #region utputNodeMouseLeftButtonDown Event
        private void OutputNodeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement outputPort)
            {
                var startPoint = GetPortCenter(outputPort);

                var currentPath = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.MediumPurple,
                    StrokeThickness = 3d,
                    Data = new PathGeometry()
                };

                Point GetPortCenter(FrameworkElement port)
                {
                    System.Windows.Controls.Canvas mainCanvas;

                    VisualTreeFinder visualTreeFinder = new();
                    var itemsControl = visualTreeFinder.FindVisualParent<ItemsControl>(outputPort);

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(itemsControl);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;

                    var point = new Point(port.Width / 2, port.Height / 2);
                    // 将当前点相对于port的坐标转换为当前点相对于Canvas的坐标位置,Canvas会先获取point左上角的位置，然后再偏移point.X,point.Y
                    var position = port.TranslatePoint(point, mainCanvas);
                    return position;
                }
            }

            e.Handled = true;
        }
        #endregion

        #region DragDropItemMouseMove Event
        private void DragDropItemMouseMove(object sender, MouseEventArgs e)
        {
           
        }
        #endregion

        private void AddPropertyChanged(DragDropItemViewModel dragDropItemViewModel)
        {
            dragDropItemViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is DragDropItemViewModel ddivm)
                {
                    if (e.PropertyName == nameof(DragDropItemViewModel.IsSelected))
                    {
                        if (ddivm.IsSelected)
                        {
                            if (_selectedDragDropItem is null)
                            {
                                _selectedDragDropItem = ddivm;
                            }

                            if (_selectedDragDropItem is not null && _selectedDragDropItem != ddivm)
                            {
                                var previewSelectedDragDropItem = _selectedDragDropItem;
                                previewSelectedDragDropItem.IsSelected = false;

                                _selectedDragDropItem = ddivm;
                            }
                        }
                        else
                        {

                        }
                    }
                }
            };
        }
        #endregion

        #region MouseLeftButtonDown Event
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Print($"DragDropView:MouseLeftButtonDown");

            if (_selectedDragDropItem is not null && !_selectedDragDropItem.IsDragging)
            {
                VisualTreeFinder visualTreeFinder = new();

                var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(_selectedDragDropItem.OriginalElement);
                var selectedNodeView = childs.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                var selectedNodeModel = selectedNodeView.DataContext as XNodeViewModel;

                _selectedDragDropItem.IsSelected = false;
                _selectedDragDropItem = null;

                selectedNodeModel.IsFocused = false;

                return;
            }

            if ((_selectedDragDropItem is not null) && _selectedDragDropItem.IsDown)
            {
                return;
            }

            System.Windows.Controls.Canvas mainCanvas;

            InitializeRectangle();
            void InitializeRectangle()
            {
                if (e.Source is ItemsControl element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;
                    var selectionRectangle = childs.FirstOrDefault(d => (d is Shape) && (d as Shape).Name == "SelectionRectangle") as Rectangle;
                    DragDropItemViewModel selectionRectangleViewModel = DragDropItems.FirstOrDefault(dd =>(dd.ViewElement is Shape) && (dd.ViewElement as Shape).Name == "SelectionRectangle");
                   
                    selectionRectangle.Visibility = Visibility.Visible;
                    selectionRectangle.Width = 0;
                    selectionRectangle.Height = 0;

                    _selectedRectangleStartPoint = e.GetPosition(mainCanvas);

                    selectionRectangleViewModel.X = _selectedRectangleStartPoint.Value.X;
                    selectionRectangleViewModel.Y = _selectedRectangleStartPoint.Value.Y;

                    mainCanvas.CaptureMouse();
                }
            }

            e.Handled = true;
        }
        #endregion

        #region MouseMove Event
        public void ExecutePreviewMouseMove(object sender, MouseEventArgs e)
        {
            //Debug.Print($"DragDropView:MouseMove");

            if ((_selectedDragDropItem is not null) && _selectedDragDropItem.IsDown)
            {
                return;
            }

            System.Windows.Controls.Canvas mainCanvas;

            if (_selectedRectangleStartPoint.HasValue && e.LeftButton == MouseButtonState.Pressed )
            {
                DragMovedRectangle();
            }

            void DragMovedRectangle()
            {
                if (e.Source is ItemsControl element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;
                    var selectionRectangle = childs.FirstOrDefault(d => (d is Shape) && (d as Shape).Name == "SelectionRectangle") as Rectangle;
                    DragDropItemViewModel selectionRectangleViewModel = DragDropItems.FirstOrDefault(dd => (dd.ViewElement is Shape) && (dd.ViewElement as Shape).Name == "SelectionRectangle");

                    var currentPosition = Mouse.GetPosition(mainCanvas);

                    double x = Math.Min(currentPosition.X, _selectedRectangleStartPoint.Value.X);
                    double y = Math.Min(currentPosition.Y, _selectedRectangleStartPoint.Value.Y);
                    double width = Math.Abs(currentPosition.X - _selectedRectangleStartPoint.Value.X);
                    double height = Math.Abs(currentPosition.Y - _selectedRectangleStartPoint.Value.Y);

                    selectionRectangleViewModel.X = x;
                    selectionRectangleViewModel.Y = y;
                    selectionRectangleViewModel.Width = width;
                    selectionRectangleViewModel.Height = height;
                    selectionRectangle.Width = width;
                    selectionRectangle.Height = height;

                  //  Debug.Print($"Width:{selectionRectangleViewModel.Width} Height:{selectionRectangleViewModel.Height}");
                }
            }
        }
        #endregion

        #region MouseLeftButtonUp Event
        public void ExecutePreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Debug.Print($"DragDropView:MouseLeftButtonUp");

            if ((_selectedDragDropItem is not null) && _selectedDragDropItem.IsDown)
            {
                return;
            }

            System.Windows.Controls.Canvas mainCanvas;

            if (_selectedRectangleStartPoint.HasValue)
            {
                if (e.Source is ItemsControl element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;

                    DragFinished();
                }
            }

            e.Handled = true;

            void DragFinished(bool cancelled = false)
            {
                mainCanvas.ReleaseMouseCapture();
                Mouse.Capture(null);

                _selectedRectangleStartPoint=null;
            }
        }
        #endregion
    }
}
