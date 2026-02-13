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
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class DragDropViewModel : BindableBase
    {
        #region Members
        private List<DragDropItemViewModel> _isFocusedDragDropItems;
        private DragDropItemViewModel _selectedDragDropItem;
        private readonly UIElement _canvas;
        private Point? _selectedRectangleStartPoint;
        private ConnectionInformation _connectionInformation;
        #endregion

        #region Constructors
        public DragDropViewModel()
        {
            DragDropItems = new();
            _isFocusedDragDropItems = new();

            AddSelectionRectangle();
            void AddSelectionRectangle()
            {
                Rectangle rectangle = new()
                {
                    Name = "SelectionRectangle",
                    Height = 0,
                    Width = 0,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeDashArray = new() { 2d, 4d },
                    Fill = new SolidColorBrush(Color.FromScRgb(0x40, 0x00, 0xA0, 0xFF)),
                    Visibility = Visibility.Collapsed
                };
                DragDropItem dragDropItem = new() { X = 0, Y = 0, Width = 0, Height = 0 };
                DragDropItemViewModel dragDropItemViewModel = new(dragDropItem) { ViewElement = rectangle };
                AddPropertyChanged(dragDropItemViewModel);
                DragDropItems.Add(dragDropItemViewModel);
            }

            _connectionInformation= new();
        }
        #endregion

        #region Properties
        public ObservableCollection<DragDropItemViewModel> DragDropItems { get; }
        public DragDropItemViewModel PreviewSelectedDragDropItem { get; private set; }

        private ContextMenu _popupMenu;
        public ContextMenu PopupMenu
        {
            get => _popupMenu;
            set => SetProperty(ref _popupMenu, value);
        }
        #endregion

        #region Drop Event
        public void ExecuteDrop(object sender, DragEventArgs e)
        {
            var menuItem = e.Data.GetData(typeof(Infrastructure.MenuItem)) as Infrastructure.MenuItem;

            var point = e.GetPosition((IInputElement)e.Source);

            DragDropItem dragDropItem = new() { X = point.X, Y = point.Y, Width = menuItem.Width, Height = menuItem.Height, ViewName = menuItem.ViewName };
            DragDropItemViewModel dragDropItemViewModel = new(dragDropItem);
            dragDropItemViewModel.MouseMove += DragDropItemMouseMove;
            AddPropertyChanged(dragDropItemViewModel);
            DragDropItems.Add(dragDropItemViewModel);

            VisualTreeFinder visualTreeFinder = new();
            var childs = visualTreeFinder.FindLogicalChilds<System.Windows.DependencyObject>(dragDropItemViewModel.ViewElement);
            var nodeView = childs.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
            var inputPort = childs.FirstOrDefault(d => (d is System.Windows.Controls.Border) && (d as System.Windows.Controls.Border).Name == "InputNode") as System.Windows.Controls.Border;
            _connectionInformation.InputPorts.Add(inputPort);
            var nodeModel = nodeView.DataContext as XNodeViewModel;
            nodeModel.OutputNodeMouseLeftButtonDown += OutputNodeMouseLeftButtonDown;
        }
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

                                //VisualTreeFinder visualTreeFinder = new();
                                //var childsInDragDropItemView = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(previewSelectedDragDropItem.OriginalElement);
                                //var previewSelectedNodeView = childsInDragDropItemView.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                                //var previewSelectedNodeModel = previewSelectedNodeView.DataContext as XNodeViewModel;

                                previewSelectedDragDropItem.IsSelected = false;
                                //   previewSelectedNodeModel.IsFocused = false;

                                _selectedDragDropItem = ddivm;
                            }
                        }
                        else
                        {
                        }

                        if (!_isFocusedDragDropItems.Any(dd => dd == ddivm))
                        {
                            VisualTreeFinder visualTreeFinder = new();
                            var childsInDragDropItemView = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(ddivm.OriginalElement);
                            var nodeView = childsInDragDropItemView.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                            var nodeModel = nodeView.DataContext as XNodeViewModel;

                            if (nodeModel.IsFocused)
                            {
                                _isFocusedDragDropItems.Add(ddivm);
                            }
                        }
                    }
                }
            };
        }
        #endregion

        #region OutputNode MouseLeftButtonDown Event
        private void OutputNodeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border outputPort)
            {
                System.Windows.Controls.Canvas mainCanvas;

                var startPoint = GetPortCenter();

                var currentPath = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.MediumPurple,
                    StrokeThickness = 3d,
                    Data = new PathGeometry()
                };

                DragDropItem dragDropItem = new() { X = startPoint.X, Y = startPoint.Y, Width = currentPath.Width, Height = currentPath.Height };
                DragDropItemViewModel dragDropItemViewModel = new(dragDropItem) { ViewElement = currentPath };
                //AddPropertyChanged(dragDropItemViewModel);
                DragDropItems.Add(dragDropItemViewModel);

                _connectionInformation.StartPoint = startPoint;
                _connectionInformation.CurrentPath = currentPath;
                _connectionInformation.IsConnecting = true;
                _connectionInformation.OutputPortRef = outputPort;

                System.Windows.Point GetPortCenter()
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var itemsControl = visualTreeFinder.FindVisualParent<ItemsControl>(outputPort);
                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(itemsControl);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;
                    var dragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(outputPort);
                    var inputPor = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(dragDropItemView).FirstOrDefault(d => (d is System.Windows.Controls.Border) && (d as System.Windows.Controls.Border).Name == "InputNode");
                    var dragDropItemViewModel = dragDropItemView.DataContext as DragDropItemViewModel;
                    var nodeView = visualTreeFinder.FindVisualChilds<XNodeView>(dragDropItemView).FirstOrDefault();

                    var centerPoint = new System.Windows.Point(outputPort.Width / 2, outputPort.Height / 2);
                    // 将当前点相对于port的坐标转换为当前点相对于Canvas的坐标位置,Canvas会先获取point左上角的位置，然后再偏移point.X,point.Y
                   //  var position = outputPort.TranslatePoint(centerPoint, mainCanvas);
                    var mainCanvasPosition = Mouse.GetPosition(mainCanvas);
                    var dragDropItemViewPoint = Mouse.GetPosition(dragDropItemView);
                    var nodeViewPoint = Mouse.GetPosition(nodeView);
                    var outputPortPoint = Mouse.GetPosition(outputPort);

                    //   var position = new System.Windows.Point(dragDropItemViewModel.X+ centerPoint.X, dragDropItemViewModel.Y+ centerPoint.Y);
                    var dragDropItemPoint = new System.Windows.Point(dragDropItemViewModel.X, dragDropItemViewModel.Y);
                    var position = dragDropItemView.TranslatePoint(dragDropItemPoint, mainCanvas);
                    //var position1 = outputPort.TranslatePoint(centerPoint, mainCanvas);

                    return dragDropItemPoint;
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

        #region MouseRightButtonDown Event
        public void ExecuteMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedDragDropItem is not null && (!_selectedDragDropItem.IsDown || !_selectedDragDropItem.IsDragging))
            {
                if (e.Source is ItemsControl itemsControl)
                {
                    CreateContextMenu();
                }

                void CreateContextMenu()
                {
                    var contextMenu = new ContextMenu();

                    MenuItem deleteNodeMenuItem = new MenuItem() { Header = "Delete Node" };
                    deleteNodeMenuItem.Click += (sender, e) =>
                    {
                        DragDropItems.Remove(_selectedDragDropItem);

                        contextMenu.Items.Remove(deleteNodeMenuItem);
                        PopupMenu = null;
                    };
                    contextMenu.Items.Add(deleteNodeMenuItem);

                    PopupMenu = contextMenu;
                }
            }
        }
        #endregion

        #region MouseLeftButtonDown Event
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Print($"DragDropView:MouseLeftButtonDown");

            System.Windows.Controls.Canvas mainCanvas=default;

            var otherDragDropItems = _isFocusedDragDropItems.Where(dd => dd != _selectedDragDropItem).ToList();
            if (otherDragDropItems.Any())
            {
                foreach (var ddim in otherDragDropItems)
                {
                    var nodeModel=  FindNodeModel(ddim.ViewElement);
                    if (nodeModel is not null)
                    {
                        nodeModel.IsFocused = false;
                    }
                }
            }

            if (_selectedDragDropItem is not null && (!_selectedDragDropItem.IsDown || !_selectedDragDropItem.IsDragging))
            {
                var nodeModel = FindNodeModel(_selectedDragDropItem.ViewElement);
                if (nodeModel is not null)
                {
                    nodeModel.IsFocused = false;
                }

                _selectedDragDropItem.IsSelected = false;
                _selectedDragDropItem = null;

                return;
            }

            XNodeViewModel FindNodeModel(DependencyObject viewElement)
            {
                if (e.Source is ItemsControl  itemsControl)
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var childsInDragDropItemView = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(viewElement);
                    var childsInNodeViewOwner = childsInDragDropItemView.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                    var nodeModel = childsInNodeViewOwner.DataContext as XNodeViewModel;

                    return nodeModel;
                }

                return null;
            }

            if ((_selectedDragDropItem is not null) && (_selectedDragDropItem.IsDown || _selectedDragDropItem.IsDragging))
            {
                return;
            }

            if ((_selectedDragDropItem is not null) && (_selectedDragDropItem.IsDown || _selectedDragDropItem.IsDragging))
            {
                return;
            }

            InitializeRectangle();
            void InitializeRectangle()
            {
                if (e.Source is ItemsControl element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;
                    var selectionRectangle = childs.FirstOrDefault(d => (d is Shape) && (d as Shape).Name == "SelectionRectangle") as Rectangle;
                    DragDropItemViewModel selectionRectangleViewModel = DragDropItems.FirstOrDefault(dd => (dd.ViewElement is Shape) && (dd.ViewElement as Shape).Name == "SelectionRectangle");

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
            System.Windows.Controls.Canvas mainCanvas;

            if (_selectedDragDropItem is not null && (!_selectedDragDropItem.IsDown || !_selectedDragDropItem.IsDragging))
            {
                return;
            }

            if (_connectionInformation.IsConnecting && _connectionInformation.CurrentPath is not null)
            {
                DragMovedPath();
            }

            void DragMovedPath()
            {
                if (e.Source is ItemsControl itemsControl)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childsInItemsControl = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(itemsControl);
                    mainCanvas = childsInItemsControl.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;

                    Point mainCanvasPoint = e.GetPosition(mainCanvas);
                    var outputPortPoint = Mouse.GetPosition(_connectionInformation.OutputPortRef);
                    Vector offset = mainCanvasPoint - outputPortPoint;

                    PathGeometry geometry = new ();
                    PathFigure figure = new (){ StartPoint = _connectionInformation.StartPoint };
                    var segment = CreateSegment("polyline", _connectionInformation.StartPoint, mainCanvasPoint);
                    figure.Segments.Add(segment);
                    geometry.Figures.Add(figure);
                    _connectionInformation.CurrentPath.Data = geometry;
                }
            }
           
            //if (_selectedRectangleStartPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            //{
            //    DragMovedRectangle();
            //}

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

        #region  CreateSegment Event
        private PathSegment CreateSegment(string type, Point startPoint, Point endPoint)
        {
            if (string.IsNullOrEmpty(type))
                throw new Exception("type 类型不能为空");
            PathSegment segment;
            if (type == "polyline")
            {
                if (startPoint.X <= endPoint.X - 40) // 两边距离大于40
                {
                    double centerX = (startPoint.X + endPoint.X) / 2;
                    var polyline = new PolyLineSegment
                    {
                        Points = new PointCollection()
                        {
                            new Point(centerX,startPoint.Y),
                            new Point(centerX,endPoint.Y),
                            new Point(endPoint.X,endPoint.Y)    // 终点
                        }
                    };
                    segment = polyline;
                }
                else
                {
                    double centerY = (startPoint.Y + endPoint.Y) / 2;
                    var polyline = new PolyLineSegment
                    {
                        Points = new PointCollection()
                        {
                            new Point(startPoint.X + 20,startPoint.Y),
                            new Point(startPoint.X + 20,centerY),
                            new Point(endPoint.X - 20,centerY),
                            new Point(endPoint.X - 20,endPoint.Y),
                            new Point(endPoint.X,endPoint.Y)    // 终点
                        }
                    };
                    segment = polyline;
                }
            }
            else
            {
                var bezier = new BezierSegment
                {
                    Point1 = new Point(startPoint.X + 50, startPoint.Y),
                    Point2 = new Point(endPoint.X - 50, endPoint.Y),
                    Point3 = endPoint
                };
                segment = bezier;
            }

            return segment;
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

            if (_connectionInformation.IsConnecting && _connectionInformation.CurrentPath is not null)
            {
                if (e.Source is ItemsControl element)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var childs = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(element);
                    mainCanvas = childs.FirstOrDefault(d => (d is System.Windows.Controls.Canvas) && (d as System.Windows.Controls.Canvas).Name == "MainCanvas") as System.Windows.Controls.Canvas;

                    var originalDragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(_connectionInformation.OutputPortRef);
                    var originalDragDropItemViewModel = originalDragDropItemView.DataContext as DragDropItemViewModel;
                    var nodeView = visualTreeFinder.FindVisualChilds<XNodeView>(originalDragDropItemView).FirstOrDefault();
                    var inputNodeRef = visualTreeFinder.FindVisualChilds<System.Windows.Controls.Border>(nodeView).FirstOrDefault(d => (d is System.Windows.Controls.Border) && (d as System.Windows.Controls.Border).Name == "InputNode");

                    var mainCanvasPosition = Mouse.GetPosition(mainCanvas);
                    var centerPoint = new System.Windows.Point(inputNodeRef.Width / 2, inputNodeRef.Height / 2);
                    var position = inputNodeRef.TranslatePoint(centerPoint, mainCanvas);

                    FrameworkElement nearestPort = null;
                    double minDist = double.MaxValue;

                    foreach (var port in _connectionInformation.InputPorts)
                    {
                        if (port == inputNodeRef)
                        {
                            continue;
                        }

                        Point portCenter = GetPortCenter(port);

                        double dist = (portCenter - mainCanvasPosition).Length;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearestPort = port;
                        }
                    }

                    if (nearestPort != null && minDist < 100) // 连线和接口的可吸附距离
                    {
                        Point endPoint = GetPortCenter(nearestPort);

                        var pathDragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(_connectionInformation.CurrentPath);
                        var pathDragDropItemViewModel = pathDragDropItemView.DataContext as DragDropItemViewModel;

                        var geometry = new PathGeometry();
                        var figure = new PathFigure { StartPoint = _connectionInformation.StartPoint };
                        var segment = CreateSegment("polyline", _connectionInformation.StartPoint, endPoint);
                        figure.Segments.Add(segment);
                        geometry.Figures.Add(figure);
                        _connectionInformation.CurrentPath.Data = geometry;

                        pathDragDropItemViewModel.X = endPoint.X;
                        pathDragDropItemViewModel.Y = endPoint.Y;

                        _connectionInformation.Connections.Add(new Connection
                        {
                            FromPort = _connectionInformation.OutputPortRef,
                            ToPort = nearestPort,
                            Path = _connectionInformation.CurrentPath
                        });
                    }
                    else
                    {
                        // 拖空则移除
                        var pathDragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(_connectionInformation.CurrentPath);
                        var pathDragDropItemViewModel = pathDragDropItemView.DataContext as DragDropItemViewModel;
                        DragDropItems.Remove(pathDragDropItemViewModel);
                    }

                    _connectionInformation.IsConnecting = false;
                    _connectionInformation.CurrentPath = null;
                }

                Point GetPortCenter(FrameworkElement port)
                {
                    var point = new Point(port.Width / 2, port.Height / 2);
                    // 将当前点相对于port的坐标转换为当前点相对于Canvas的坐标位置,Canvas会先获取point左上角的位置，然后再偏移point.X,point.Y
                    var position = port.TranslatePoint(point, mainCanvas);
                    return position;
                }
            }

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

                _selectedRectangleStartPoint = null;
            }
        }
        #endregion
    }
}
