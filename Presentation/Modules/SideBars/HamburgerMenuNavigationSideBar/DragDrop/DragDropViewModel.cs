using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Unity;

using Prism;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;

using Aksl.Toolkit.Services;
using Aksl.Toolkit.UI;
using Aksl.ViewModels;
using Aksl.Views;
using Aksl.Infrastructure;

using Aksl.Modules.HamburgerMenuNavigationSideBar.Views;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class DragDropViewModel : BindableBase
    {
        #region Members
        private readonly IDialogViewService _dialogViewService;
        private List<DragDropItemViewModel> _isFocusedDragDropItems;
        private DragDropItemViewModel _selectedDragDropItem;
        private readonly UIElement _canvas;
        private Point? _selectedRectangleStartPoint;
        private ConnectionInformation _connectionInformation;
        #endregion

        #region Constructors
        public DragDropViewModel()
        {
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

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

            _connectionInformation = new();
            Connections = new();
            InputPorts = new();
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

        public List<Connection> Connections { get; set; }
        public List<Border> InputPorts { get; set; }
        #endregion

        #region Drop Event
        public void ExecuteDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (sender is ItemsControl itemsControl)
                {
                    var menuItem = e.Data.GetData(typeof(Infrastructure.MenuItem)) as Infrastructure.MenuItem;

                    VisualTreeFinder visualTreeFinder = new();

                    var mainCanvas = visualTreeFinder.FindVisualChild<Canvas>(itemsControl);
                    var currentPoint = e.GetPosition(mainCanvas);

                    DoDrop();
                    void DoDrop()
                    {
                        DragDropItem dragDropItem = new() { X = currentPoint.X, Y = currentPoint.Y, Width = menuItem.Width, Height = menuItem.Height, ViewName = menuItem.ViewName };
                        DragDropItemViewModel dragDropItemViewModel = new(dragDropItem);
                        dragDropItemViewModel.MouseMove += DragDropItemMouseMove;
                        AddPropertyChanged(dragDropItemViewModel);
                        DragDropItems.Add(dragDropItemViewModel);

                        if (dragDropItemViewModel.ViewElement is not null)
                        {
                            var nodeView = visualTreeFinder.FindLogicalChild<XNodeView>(dragDropItemViewModel.ViewElement);
                            // nodeView.PopupMenu = CreateNodeContextMenu();
                            var nodeModel = nodeView?.DataContext as XNodeViewModel;
                            //nodeModel.PopupMenu = CreateNodeContextMenu();
                            nodeModel.MouseRightButtonDown += ExecuteNodeMouseRightButtonDown;
                            nodeModel.OutputNodePreviewMouseLeftButtonDown += ExecuteOutputNodePreviewMouseLeftButtonDown;

                            var borders = visualTreeFinder.FindLogicalChilds<Border>(dragDropItemViewModel.ViewElement);
                            var inputPortBorder = borders.FirstOrDefault(b => b.Name == "InputNode");
                            InputPorts.Add(inputPortBorder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogViewService.AlertAsync(message: $"ExecuteDrop Error: \"{ex.Message}\"", title: "Error").Await();
            }
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
                            SetSelectedDragDropItem();
                            void SetSelectedDragDropItem()
                            {
                                if (_selectedDragDropItem is null)
                                {
                                    _selectedDragDropItem = ddivm;
                                }

                                if (_selectedDragDropItem is not null && _selectedDragDropItem != ddivm)
                                {
                                    var previewSelectedDragDropItem = _selectedDragDropItem;

                                    VisualTreeFinder visualTreeFinder = new();
                                    var previewSelectedNodeView = visualTreeFinder.FindVisualChild<XNodeView>(previewSelectedDragDropItem.ViewElement);
                                    if (previewSelectedNodeView is not null)
                                    {
                                        var previewSelectedNodeModel = previewSelectedNodeView.DataContext as XNodeViewModel;
                                        previewSelectedNodeModel.IsFocused = false;
                                    }

                                    previewSelectedDragDropItem.IsSelected = false;

                                    _selectedDragDropItem = ddivm;
                                }
                            }
                        }
                        else
                        {
                        }

                        //if (!_isFocusedDragDropItems.Any(dd => dd == ddivm))
                        //{
                        //    VisualTreeFinder visualTreeFinder = new();
                        //    var childsInDragDropItemView = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(ddivm.OriginalElement);
                        //    var nodeView = childsInDragDropItemView.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                        //    var nodeModel = nodeView.DataContext as XNodeViewModel;

                        //    if (nodeModel.IsFocused)
                        //    {
                        //        _isFocusedDragDropItems.Add(ddivm);
                        //    }
                        //}
                    }
                }
            };
        }

        private ContextMenu CreateNodeContextMenu()
        {
            ContextMenu contextMenu = new();

            var editNodeMenuItem = new System.Windows.Controls.MenuItem() { Header = "Edit Node", FontSize = 13 };
            editNodeMenuItem.Click += (sender, e) =>
            {
                if (HasSelectedDragDropItem())
                {
                    var dialogService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<Aksl.Services.Dialogs.DialogService>();

                    VisualTreeFinder visualTreeFinder = new();
                    var nodeView = visualTreeFinder.FindLogicalChild<XNodeView>(_selectedDragDropItem.ViewElement);
                    if (nodeView is not null)
                    {
                        var nodeViewModel = nodeView.DataContext as XNodeViewModel;

                        EditXNodeView editXNodeView = new();
                        var editXNodeViewModel = editXNodeView.DataContext as EditXNodeViewModel;
                        editXNodeViewModel.HeaderBackgroundColor = nodeViewModel.HeaderBackgroundColor;
                        editXNodeViewModel.Content = nodeViewModel.Content;
                        editXNodeViewModel.LineWidth = nodeViewModel.LineWidth;

                        var parameters = new DialogParameters { { "Title", $"Edit:{editXNodeViewModel.Content}" }, { "OkText", "确定" }, { "CancelText", "取消" } };
                        dialogService.ShowDialog(editXNodeView, parameters: parameters, windowName: nameof(Toolkit.Dialogs.FixedSizeDialogWindow), callback: (result) =>
                        {
                            if (result.Parameters.TryGetValue("NodeViewModel", out EditXNodeViewModel editNodeViewModel))
                            {
                                nodeViewModel.HeaderBackgroundColor = editNodeViewModel?.HeaderBackgroundColor;
                                nodeViewModel.Content = editNodeViewModel.Content;
                                nodeViewModel.LineWidth = editNodeViewModel.LineWidth;
                            }
                        });
                    }
                }
            };
            contextMenu.Items.Add(editNodeMenuItem);

            var deleteNodeMenuItem = new System.Windows.Controls.MenuItem() { Header = "Delete Node", FontSize = 13 };
            deleteNodeMenuItem.Click += (sender, e) =>
            {
                if (HasSelectedDragDropItem())
                {
                    DragDropItems.Remove(_selectedDragDropItem);
                }
            };
            contextMenu.Items.Add(deleteNodeMenuItem);

            bool HasSelectedDragDropItem() => _selectedDragDropItem is not null && (DragDropItems.Contains(_selectedDragDropItem));

            //void ExecuteEditNodeCommand()
            //{
            //    if (_selectedDragDropItem is not null && (DragDropItems.Contains(_selectedDragDropItem)))
            //    {
            //        // await _dialogViewService.ConfirmAsync(message: $"Edit.: \"{_selectedDragDropItem.ViewName}\"", title: "Message", okText: "确定", cancelText:"取消", callBack:null);
            //        var dialogService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<Aksl.Services.Dialogs.DialogService>();
            //        var parameters = new DialogParameters { { "IsConfirm", true }, { "Message", "message" }, { "Title", "Edit" }, { "OkText", "确定" }, { "CancelText", "取消" } };

            //        VisualTreeFinder visualTreeFinder = new();
            //        var nodeView = visualTreeFinder.FindLogicalChild<XNodeView>(_selectedDragDropItem.ViewElement);
            //        if (nodeView is not null)
            //        {
            //            var nodeViewModel = nodeView.DataContext as XNodeViewModel;

            //            EditXNodeView editXNodeView = new();
            //            var editXNodeViewModel = editXNodeView.DataContext as EditXNodeViewModel;
            //            editXNodeViewModel.HeaderBackgroundColor = nodeViewModel.HeaderBackgroundColor;
            //            editXNodeViewModel.Content = nodeViewModel.Content;

            //            dialogService.ShowDialog(editXNodeView, parameters: parameters, callback: null, windowName: nameof(Toolkit.Dialogs.FixedSizeDialogWindow));
            //        }
            //    }
            //}

            //bool CanExecuteEditNodeCommand()
            //{
            //    bool canExecute = _selectedDragDropItem is not null && DragDropItems.Contains(_selectedDragDropItem);
            //    return canExecute;
            //}

            //void ExecuteDeleteNodeCommand()
            //{
            //    if (_selectedDragDropItem is not null && (DragDropItems.Contains(_selectedDragDropItem)))
            //    {
            //        DragDropItems.Remove(_selectedDragDropItem);
            //    }
            //}

            return contextMenu;
        }
        #endregion

        #region DragDropItem MouseMove Event
        private void DragDropItemMouseMove(object sender, MouseEventArgs e)
        {
            Canvas mainCanvas;
            VisualTreeFinder visualTreeFinder = new();

            if (sender is DragDropItemView dragDropItemView)
            {
                if (Connections.Any() && IsDragging())
                {
                    mainCanvas = visualTreeFinder.FindVisualParent<Canvas>(dragDropItemView);

                    System.Windows.Point startPoint = default;
                    System.Windows.Point endPoint = default;

                    var refConns = Connections.Where(c => IsChildOf(c.FromPort, _selectedDragDropItem.ViewElement) || IsChildOf(c.ToPort, _selectedDragDropItem.ViewElement)).ToList();

                    if (refConns.Any())
                    {
                        foreach (var refConn in refConns)
                        {
                            startPoint = GetPortCenter(refConn.FromPort);
                            endPoint = GetPortCenter(refConn.ToPort);

                            DoPolyLineSegment();
                            void DoPolyLineSegment()
                            {
                                var points = CreatePolyPoints(startPoint, endPoint);
                                var polyLineSegmentViewModel = refConn.ShapeElementViewModel as PolyLineSegmentViewModel;
                                polyLineSegmentViewModel.StartPoint = startPoint;
                                polyLineSegmentViewModel.Points = points;
                            }

                            DoDragDropItem();
                            void DoDragDropItem()
                            {
                                double moveX = Math.Min(startPoint.X, endPoint.X);
                                double moveY = Math.Min(startPoint.Y, endPoint.Y);
                                double moveWidth = Math.Abs(endPoint.X - startPoint.X);
                                double moveHeight = Math.Abs(endPoint.Y - startPoint.Y);
                                refConn.DragDropItemViewModel.X = moveX;
                                refConn.DragDropItemViewModel.Y = moveY;
                                refConn.DragDropItemViewModel.Width = moveWidth;
                                refConn.DragDropItemViewModel.Height = moveHeight;
                                refConn.ShapeElement.Width = moveWidth;
                                refConn.ShapeElement.Height = moveHeight;
                            }
                        }
                    }
                }
            }

            bool IsDragging() => _selectedDragDropItem is not null && (_selectedDragDropItem.IsDown || _selectedDragDropItem.IsDragging);

            bool IsChildOf(FrameworkElement child, DependencyObject parent)
            {
                var childs = visualTreeFinder.FindVisualChilds<DependencyObject>(parent);
                return childs.Contains(child);
            }

            System.Windows.Point GetPortCenter(FrameworkElement portBorder)
            {
                var centerPoint = new System.Windows.Point(portBorder.Width / 2, portBorder.Height / 2);
                var pointRelativeTo = portBorder.TranslatePoint(centerPoint, mainCanvas);
                return pointRelativeTo;
            }
        }
        #endregion

        #region Node MouseRightButtonDown Event
        public void ExecuteNodeMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.Print($"Node MouseRightButtonDown");
            // PopupMenu = null;

            VisualTreeFinder visualTreeFinder = new();

            if (e.Source is XNodeView nodeView)
            {
                var dragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(nodeView);

                var nodeViewModel = nodeView.DataContext as XNodeViewModel;
                nodeViewModel.PopupMenu = null;

                //if (!HasSelectedDragDropItem())
                //{
                //    return;
                //}

                CreateContextMenu();
            }

            void CreateContextMenu()
            {
                var nodeViewModel = nodeView.DataContext as XNodeViewModel;

                // ContextMenu contextMenu = new() { Visibility = HasSelectedDragDropItem() ? Visibility.Visible : Visibility.Collapsed };
                ContextMenu contextMenu = new();

                var editNodeMenuItem = new System.Windows.Controls.MenuItem() { Header = "Edit Node", FontSize = 13 };
                editNodeMenuItem.Click += (sender, e) =>
                {
                    //if (HasSelectedDragDropItem())
                    //{
                    var dialogService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<Aksl.Services.Dialogs.DialogService>();

                    //var nodeView = visualTreeFinder.FindLogicalChild<XNodeView>(_selectedDragDropItem.ViewElement);
                    //if (nodeView is not null)
                    //{
                    EditXNodeView editXNodeView = new();
                    var editXNodeViewModel = editXNodeView.DataContext as EditXNodeViewModel;
                    editXNodeViewModel.HeaderBackgroundColor = nodeViewModel.HeaderBackgroundColor;
                    editXNodeViewModel.Content = nodeViewModel.Content;
                    editXNodeViewModel.LineWidth = nodeViewModel.LineWidth;

                    var parameters = new DialogParameters { { "Title", $"Edit:{editXNodeViewModel.Content}" }, { "OkText", "确定" }, { "CancelText", "取消" } };
                    dialogService.ShowDialog(editXNodeView, parameters: parameters, windowName: nameof(Toolkit.Dialogs.FixedSizeDialogWindow), callback: (result) =>
                    {
                        if (result.Parameters.TryGetValue("NodeViewModel", out EditXNodeViewModel editNodeViewModel))
                        {
                            nodeViewModel.HeaderBackgroundColor = editNodeViewModel?.HeaderBackgroundColor;
                            nodeViewModel.Content = editNodeViewModel.Content;
                            nodeViewModel.LineWidth = editNodeViewModel.LineWidth;
                        }
                    });
                    //}
                    //}
                };
                contextMenu.Items.Add(editNodeMenuItem);

               var deleteNodeMenuItem = new System.Windows.Controls.MenuItem() { Header = "Delete Node", FontSize = 13 };
                deleteNodeMenuItem.Click += (sender, e) =>
                {
                    var dragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(nodeView);
                    var dragDropItemViewModel = dragDropItemView.DataContext as DragDropItemViewModel;
                    var borders = visualTreeFinder.FindVisualChilds<Border>(nodeView);
                    Border inputNodeRef = borders.FirstOrDefault(d => d.Name == "InputNode");
                    Border outputNodeRef = borders.FirstOrDefault(d => d.Name == "OutputNode");

                    DoConnections();
                    void DoConnections()
                    {
                        if (Connections.Any())
                        {
                            var refConns = Connections.Where(c => IsChildOf(c.FromPort, nodeView) || IsChildOf(c.ToPort, nodeView)).ToList();

                            if (refConns.Any())
                            {
                                var dragDropItemViewModelToShapes = refConns.Select(c => c.DragDropItemViewModel).ToList();

                                foreach (var dd in dragDropItemViewModelToShapes)
                                {
                                    DragDropItems.Remove(dd);
                                }

                                foreach (var c in refConns)
                                {
                                    Connections.Remove(c);
                                }
                            }
                        }
                    }

                    InputPorts.Remove(inputNodeRef);
                    DragDropItems.Remove(dragDropItemViewModel);

                    //if (HasSelectedDragDropItem())
                    //{
                    //    DragDropItems.Remove(_selectedDragDropItem);
                    //}
                };
                contextMenu.Items.Add(deleteNodeMenuItem);

                nodeViewModel.PopupMenu = contextMenu;
            }

            bool IsChildOf(FrameworkElement child, DependencyObject parent)
            {
                var childs = visualTreeFinder.FindVisualChilds<DependencyObject>(parent);
                return childs.Contains(child);
            }

            // bool HasSelectedDragDropItem() => _selectedDragDropItem is not null && (DragDropItems.Contains(_selectedDragDropItem));

            e.Handled = true;
        }
        #endregion

        #region ItemsControl MouseRightButtonDown Event
        public void ExecuteMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.Print($"ItemsControl MouseRightButtonDown");

            if (e.Source is ItemsControl itemsControl)
            {
                PopupMenu = null;

                var itemsControlPoint = Mouse.GetPosition(itemsControl);

                CreateContextMenu(itemsControlPoint);
            }

            void CreateContextMenu(Point position)
            {
                ContextMenu contextMenu = new();

                System.Windows.Controls.MenuItem addNodeMenuItem = new() { Header = "Add Node", FontSize = 13 };

                addNodeMenuItem.Click += (sender, e) =>
                {
                    DragDropItem dragDropItem = new() { X = position.X, Y = position.Y, Width = 240, Height = 160, ViewName = "Aksl.Modules.Functions.Views.RelationOperationView,Aksl.Modules.Functions" };
                    DragDropItemViewModel dragDropItemViewModel = new(dragDropItem);
                    dragDropItemViewModel.MouseMove += DragDropItemMouseMove;
                    AddPropertyChanged(dragDropItemViewModel);
                    DragDropItems.Add(dragDropItemViewModel);

                    VisualTreeFinder visualTreeFinder = new();
                    var childsInNodeViewOwner = visualTreeFinder.FindLogicalChilds<DependencyObject>(dragDropItemViewModel.ViewElement);
                    if (childsInNodeViewOwner != null)
                    {
                        var nodeView = childsInNodeViewOwner.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                        //nodeView.PopupMenu = CreateNodeContextMenu();
                        var nodeModel = nodeView?.DataContext as XNodeViewModel;
                        nodeModel.MouseRightButtonDown += ExecuteNodeMouseRightButtonDown;
                        nodeModel.OutputNodePreviewMouseLeftButtonDown += ExecuteOutputNodePreviewMouseLeftButtonDown;

                        var inputPort = childsInNodeViewOwner.FirstOrDefault(d => (d is Border) && (d as Border).Name == "InputNode") as Border;
                        InputPorts.Add(inputPort);
                    }
                };
                contextMenu.Items.Add(addNodeMenuItem);

                PopupMenu = contextMenu;
            }

            e.Handled = true;
        }
        #endregion

        #region ItemsControl MouseLeftButtonDown Event
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.Print($"DragDropView:MouseLeftButtonDown");

            System.Windows.Controls.Canvas mainCanvas = default;

            //var otherDragDropItems = _isFocusedDragDropItems.Where(dd => dd != _selectedDragDropItem).ToList();
            //if (otherDragDropItems.Any())
            //{
            //    foreach (var ddim in otherDragDropItems)
            //    {
            //        var nodeModel = FindNodeModel(ddim.ViewElement);
            //        if (nodeModel is not null)
            //        {
            //            nodeModel.IsFocused = false;
            //        }
            //    }
            //}

            if (_selectedDragDropItem is not null && (!_selectedDragDropItem.IsDown || !_selectedDragDropItem.IsDragging))
            {
                //var nodeModel = FindNodeModel(_selectedDragDropItem.ViewElement);
                //if (nodeModel is not null)
                //{
                //    nodeModel.IsFocused = false;
                //}
                ClearSelectedDragDropItem();
                void ClearSelectedDragDropItem()
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var nodeView = visualTreeFinder.FindVisualChild<XNodeView>(_selectedDragDropItem.ViewElement);
                    if (nodeView is not null)
                    {
                        var nodeModel = nodeView.DataContext as XNodeViewModel;
                        nodeModel.IsFocused = false;
                    }

                    _selectedDragDropItem.IsSelected = false;
                    _selectedDragDropItem = null;
                }

                return;
            }

            XNodeViewModel FindNodeModel(DependencyObject viewElement)
            {
                if (e.Source is ItemsControl itemsControl)
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var nodeView = visualTreeFinder.FindVisualChild<XNodeView>(viewElement);
                    //var allChilds = visualTreeFinder.FindVisualChilds<System.Windows.DependencyObject>(viewElement);
                    //var nodeView = allChilds.FirstOrDefault(d => (d is XNodeView)) as XNodeView;
                    var nodeModel = nodeView.DataContext as XNodeViewModel;

                    return nodeModel;
                }

                return null;
            }

            // InitializeRectangle();
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

        #region OutputNode MouseLeftButtonDown Event
        private void ExecuteOutputNodePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border outputBorder)
            {
                VisualTreeFinder visualTreeFinder = new();
                var mainCanvas = visualTreeFinder.FindVisualParent<Canvas>(outputBorder);
                var dragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(outputBorder);

                var centerPoint = new System.Windows.Point(outputBorder.Width / 2, outputBorder.Height / 2);
                var startPoint = outputBorder.TranslatePoint(centerPoint, mainCanvas);
                //   var endPoint = new System.Windows.Point(startPoint.X + 1, startPoint.Y + 1);

                //   Debug.Print($"OutputNode:MouseLeftButtonDown{startPoint}");

                //var startPoint = GetOutputPortCenter(); 
                //System.Windows.Point GetOutputPortCenter()
                //{
                //    var centerPoint = new System.Windows.Point(outputBorder.Width / 2, outputBorder.Height / 2);
                //    var pointRelativeTo = outputBorder.TranslatePoint(centerPoint, mainCanvas);
                //    return pointRelativeTo;
                //}

                #region Method
                //System.Windows.Shapes.Path currentPath = new()
                //{
                //    Stroke = Brushes.MediumPurple,
                //    StrokeThickness = 3d,
                //    Data = new PathGeometry()
                //};

                //var geometry = new PathGeometry();
                //PathFigure figure = new() { StartPoint = startPoint };
                //  var bezierSegment = CreateBezierSegment();
                //   figure.Segments.Add(bezierSegment);
                //LineSegment lineSegment = new(endPoint, true) { IsSmoothJoin = true };
                //figure.Segments.Add(lineSegment);
                //geometry.Figures.Add(figure);
                //currentPath.Data = geometry;
                #endregion

                //DoBezier(); 
                void DoBezier()
                {
                    var bezierview = (PrismApplication.Current as PrismApplicationBase).Container.Resolve(typeof(BezierView)) as FrameworkElement;
                    var bezierviewModel = bezierview?.DataContext as BezierViewModel;
                    bezierviewModel.Stretch = Stretch.Fill;
                    bezierviewModel.Stroke = Brushes.MediumPurple;
                    bezierviewModel.StrokeThickness = 3d;
                    bezierviewModel.StrokeDashCap = PenLineCap.Round;
                    bezierviewModel.StartPoint = startPoint;
                    bezierviewModel.Point1 = startPoint;
                    bezierviewModel.Point2 = startPoint;
                    bezierviewModel.Point3 = startPoint;

                    DragDropItem dragDropItemToBezier = new() { X = startPoint.X, Y = startPoint.Y, Width = 0, Height = 0 };
                    DragDropItemViewModel dragDropItemViewModelToBezier = new(dragDropItemToBezier) { ViewElement = bezierview };
                    DragDropItems.Add(dragDropItemViewModelToBezier);

                    _connectionInformation.DragDropItemViewModel = dragDropItemViewModelToBezier;
                    _connectionInformation.ShapeElement = bezierview;
                    _connectionInformation.ShapeElementViewModel = bezierviewModel;
                }

                DoPolyLineSegment();
                void DoPolyLineSegment()
                {
                    var points = CreatePolyPoints(startPoint, startPoint);
                    var polyLineSegmentView = (PrismApplication.Current as PrismApplicationBase).Container.Resolve(typeof(PolyLineSegmentView)) as FrameworkElement;
                    var polyLineSegmentViewModel = polyLineSegmentView?.DataContext as PolyLineSegmentViewModel;
                    polyLineSegmentViewModel.Stretch = Stretch.Fill;
                    polyLineSegmentViewModel.Stroke = Brushes.MediumPurple;
                    polyLineSegmentViewModel.StrokeThickness = 3d;
                    polyLineSegmentViewModel.StrokeDashCap = PenLineCap.Flat;
                    polyLineSegmentViewModel.Points = points;
                    DragDropItem dragDropItemToPoly = new() { X = startPoint.X, Y = startPoint.Y, Width = 1, Height = 1 };
                    DragDropItemViewModel dragDropItemViewModelToPoly = new(dragDropItemToPoly) { ViewElement = polyLineSegmentView };
                    DragDropItems.Add(dragDropItemViewModelToPoly);

                    _connectionInformation.DragDropItemViewModel = dragDropItemViewModelToPoly;
                    _connectionInformation.ShapeElement = polyLineSegmentView;
                    _connectionInformation.ShapeElementViewModel = polyLineSegmentViewModel;

                    polyLineSegmentView.MouseLeftButtonDown += PolyLineMouseLeftButtonDown;
                    polyLineSegmentView.MouseEnter += PolyLineMouseEnter;
                    polyLineSegmentView.MouseLeave += PolyLineMouseLeave;
                }

                _connectionInformation.IsConnecting = true;
                _connectionInformation.StartPoint = startPoint;
                _connectionInformation.OutputPort = outputBorder;

                #region Method
                //var pathView = (PrismApplication.Current as PrismApplicationBase).Container.Resolve(typeof(PathView)) as FrameworkElement;
                //var pathViewModel = pathView?.DataContext as PathViewModel;
                //var geometry = new PathGeometry();
                //PathFigure figure = new() { StartPoint = startPoint };
                // var bezierSegment = CreateSegment("Bezier", startPoint, startPoint);
                //figure.Segments.Add(bezierSegment);
                //geometry.Figures.Add(figure);
                //pathViewModel.Data = geometry;
                // DragDropItem dragDropItemWithPath = new() { X = startPoint.X, Y = startPoint.Y, Width = currentPath.Data.Bounds.Width + 4, Height = currentPath.Data.Bounds.Height + 4 };
                // DragDropItem dragDropItemWithPath = new() { X = startPoint.X+ currentPath.Data.Bounds.X, Y = startPoint.Y+ currentPath.Data.Bounds.Y, Width = currentPath.Width + 4, Height = currentPath.Height + 4 };
                //  DragDropItem dragDropItemWithPath = new() { X = startPoint.X, Y = startPoint.Y, Width = currentPath.Width + 4, Height = currentPath.Height + 4 };
                // DragDropItemViewModel dragDropItemWithPathViewModel = new(dragDropItemWithPath) { ViewElement = currentPath };
                //AddPropertyChanged(dragDropItemViewModel);

                // _connectionInformation.IsConnecting = true;
                // _connectionInformation.StartPoint = startPoint;
                //// _connectionInformation.CurrentPath = currentPath;
                // _connectionInformation.CurrentDragDropItemViewModel = dragDropItemViewModelToBezier;
                // _connectionInformation.CurrentViewElement = bezierview;
                // _connectionInformation.CurrentViewModel = bezierviewModel;
                // //_connectionInformation.CurrentDragDropItemViewModel = dragDropItemViewModelToPoly;
                // //_connectionInformation.CurrentViewElement = polyLineSegmentView;
                // //_connectionInformation.CurrentViewModel = polyLineSegmentViewModel;
                // _connectionInformation.OutputPortRef = outputBorder;
                #endregion

                #region Method
                //System.Windows.Point GetOutputPortCenter()
                //{
                //    //VisualTreeFinder visualTreeFinder = new();

                //    //var mainCanvas = visualTreeFinder.FindVisualParent<Canvas>(outputBorder);
                //    //var dragDropItemView = visualTreeFinder.FindVisualParent<DragDropItemView>(outputBorder);
                //    //var dragDropItemViewModel = dragDropItemView.DataContext as DragDropItemViewModel;
                //    //var nodeView = visualTreeFinder.FindVisualParent<XNodeView>(outputBorder);
                //    //var inputPorBorder = visualTreeFinder.FindVisualChilds<DependencyObject>(nodeView).FirstOrDefault(d => (d is Border) && (d as Border).Name == "InputNode");

                //    //Rect bounds = VisualTreeHelper.GetDescendantBounds(outputBorder);

                //    ////     Debug.Print($"OutputNodeMouseLeftButtonDown.DragDropItemView:X={dragDropItemViewModel.X} Y={dragDropItemViewModel.Y}");

                //    //var centerPoint = new System.Windows.Point(outputBorder.Width / 2, outputBorder.Height / 2);
                //    //var outputLeftPoint = new System.Windows.Point(dragDropItemViewModel.X + nodeView.ActualWidth, dragDropItemViewModel.Y + nodeView.ActualHeight / 2);
                //    //var outputRelativeToCanvasPoint = outputBorder.TranslatePoint(centerPoint, mainCanvas);
                //    ////var outputBorderToCanvasPoint = outputBorder.TransformToAncestor(mainCanvas).Transform(new Point(0, 0));
                //    ////outputBorderToCanvasPoint.X = outputBorderToCanvasPoint.X + outputBorder.Width / 2;
                //    ////outputBorderToCanvasPoint.Y = outputBorderToCanvasPoint.Y + outputBorder.Height / 2;


                //    //// 将当前点相对于port的坐标转换为当前点相对于Canvas的坐标位置,Canvas会先获取point左上角的位置，然后再偏移point.X,point.Y
                //    ////  var position = outputPort.TranslatePoint(centerPoint, mainCanvas);
                //    //var mainCanvasPoint = Mouse.GetPosition(mainCanvas);
                //    ////Debug.Print($"OutputNodeMouseLeftButtonDown.DragDropItemViewModel:X={dragDropItemViewModel.X} Y={dragDropItemViewModel.Y}");
                //    ////Debug.Print($"OutputNodeMouseLeftButtonDown.Canvast:X={mainCanvasPoint.X} Y={mainCanvasPoint.Y}");
                //    ////Debug.Print($"OutputNodeMouseLeftButtonDown.DragDropItemView+NodeView:X={dragDropItemViewModel.X + nodeView.ActualWidth} Y={dragDropItemViewModel.Y + nodeView.ActualHeight}");
                //    ////   Debug.Print($"OutputNodeMouseLeftButtonDown.Canvas:X={mainCanvasPoint.X} Y={mainCanvasPoint.Y}");
                //    //var dragDropItemViewPosition = Mouse.GetPosition(dragDropItemView);
                //    //var nodeViewPosition = Mouse.GetPosition(nodeView);
                //    //var outputPortPoint = Mouse.GetPosition(outputBorder);
                //    //var dragDropItemViewPoint = MouseUtilities.GetMousePosition(dragDropItemView);//Mouse RelativeTo Border Position
                //    //// Debug.Print($"OutputNodeMouseLeftButtonDown.OutputPort):X={outputPortPoint.X} Y={outputPortPoint.Y}");

                //    ////var mainCanvasPosition = MouseUtilities.GetMousePosition(mainCanvas);//Mouse RelativeTo Canvas Position
                //    ////Debug.Print($"OutputNodeMouseLeftButtonDown.Canvast):X={mainCanvasPosition.X} Y={mainCanvasPosition.Y}");
                //    //// var outputPortPosition = MouseUtilities.GetMousePosition(outputPort);
                //    ////  Debug.Print($"OutputNodeMouseLeftButtonDown.OutputPort):X={outputPortPosition.X} Y={outputPortPosition.Y}");

                //    //// var position = new System.Windows.Point(dragDropItemViewModel.X + centerPoint.X, dragDropItemViewModel.Y + centerPoint.Y);
                //    //var dragDropItemPoint = new System.Windows.Point(dragDropItemViewModel.X, dragDropItemViewModel.Y);
                //    //var dragDropItemViewPointRelativeToCanvasPoint = dragDropItemView.TranslatePoint(new Point(0, 0), mainCanvas);
                //    ////   Debug.Print($"OutputNodeMouseLeftButtonDown.RelativeToCanvast:X={dragDropItemViewPointRelativeToCanvasPoint.X} Y={dragDropItemViewPointRelativeToCanvasPoint.Y}");
                //    //var outputPortRelativeToCanvasPoint = outputBorder.TranslatePoint(new Point(0, 0), mainCanvas);
                //    ////  Debug.Print($"OutputNodeMouseLeftButtonDown.OutputPortRelativeToCanvast:X={outputPortRelativeToCanvasPoint.X} Y={outputPortRelativeToCanvasPoint.Y}");
                //    ////var positionRelativeToDragDropItemView = nodeView.TranslatePoint(positionRelativeToNodeView, dragDropItemView);
                //    ////var positionRelativeToCanvas = dragDropItemView.TranslatePoint(dragDropItemPoint, mainCanvas);
                //    ////var positionRelativeToItemsControl = dragDropItemView.TranslatePoint(dragDropItemPoint, itemsControl);

                //    //// var position = new System.Windows.Point(dragDropItemViewModel.X - dragDropItemViewPoint.X, dragDropItemViewModel.Y - dragDropItemViewPoint.Y);
                //    // var position = new System.Windows.Point(dragDropItemViewModel.X , dragDropItemViewModel.Y);

                //    //return outputRelativeToCanvasPoint;
                //}
                #endregion
            }

            e.Handled = true;
        }
        #endregion

        #region ItemsControl MouseMove Event
        public void ExecutePreviewMouseMove(object sender, MouseEventArgs e)
        {
            //   Debug.Print($"ItemsControl:MouseMove{_connectionInformation.StartPoint}");
            Canvas mainCanvas;

            if (IsDragging())
            {
                return;
            }

            if (IsConnection())
            {
                DragMovedPath();
            }

            bool IsDragging() => _selectedDragDropItem is not null && (_selectedDragDropItem.IsDown || _selectedDragDropItem.IsDragging);

            bool IsConnection() => _connectionInformation.IsConnecting && _connectionInformation.DragDropItemViewModel is not null && _connectionInformation.ShapeElement is not null;

            void DragMovedPath()
            {
                if (e.Source is ItemsControl itemsControl)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    var mainCanvas = visualTreeFinder.FindVisualChild<Canvas>(itemsControl);
                    var currentPoint = e.GetPosition(mainCanvas);

                    Vector currentOffset = currentPoint - _connectionInformation.StartPoint;
                    if (Math.Abs(currentOffset.X) < 0.5d && Math.Abs(currentOffset.Y) < 0.5d)
                    {
                        return;
                    }

                    #region Method
                    //var pathViewModel = _connectionInformation.PathViewModel;
                    //var geometry = new PathGeometry();
                    //PathFigure figure = new() { StartPoint = _connectionInformation.StartPoint };
                    //var Data = CreateSegment("Bezier", _connectionInformation.StartPoint, mainCanvasPoint);
                    //geometry.Figures.Add(figure);
                    //pathViewModel.Data = geometry;

                    // PathGeometry geometry = new();
                    // PathFigure figure = new() { StartPoint = _connectionInformation.StartPoint };
                    // var segment = CreateSegment("BezierSegment", _connectionInformation.StartPoint, mainCanvasPoint);
                    // figure.Segments.Add(segment);
                    // //LineSegment lineSegment = new(mainCanvasPoint,true) { IsSmoothJoin = true };
                    //// figure.Segments.Add(lineSegment);
                    // geometry.Figures.Add(figure);
                    // _connectionInformation.CurrentPath.Data = geometry;

                    //var dragDropItemViewWithPath = visualTreeFinder.FindVisualParent<DragDropItemView>(_connectionInformation.CurrentPath);
                    //var dragDropItemViewModelWithPath = dragDropItemViewWithPath.DataContext as DragDropItemViewModel;
                    //dragDropItemViewModelWithPath.X = mainCanvasPoint.X;
                    //dragDropItemViewModelWithPath.Y = mainCanvasPoint.Y;

                    #endregion

                    // DoBezier();
                    void DoBezier()
                    {
                        var bezierviewModel = _connectionInformation.ShapeElementViewModel as BezierViewModel;
                        //bezierviewModel.Stretch = Stretch.Fill;
                        //bezierviewModel.Stroke = Brushes.MediumPurple;
                        //bezierviewModel.StrokeThickness = 3d;
                        //bezierviewModel.StrokeDashCap = PenLineCap.Round;
                        bezierviewModel.StartPoint = _connectionInformation.StartPoint;
                        bezierviewModel.Point1 = new(_connectionInformation.StartPoint.X + 50, _connectionInformation.StartPoint.Y);
                        bezierviewModel.Point2 = new(currentPoint.X - 50, currentPoint.Y);
                        bezierviewModel.Point3 = currentPoint;
                        //if (currentOffset.X >0d && currentOffset.Y > 0d)
                        //{
                        //    bezierviewModel.StartPoint = _connectionInformation.StartPoint;
                        //    bezierviewModel.Point1 = new(_connectionInformation.StartPoint.X + 50, _connectionInformation.StartPoint.Y);
                        //    bezierviewModel.Point2 = new(currentPoint.X - 50, currentPoint.Y);
                        //    bezierviewModel.Point3 = currentPoint;
                        //}
                        //if (currentOffset.X > 0d && currentOffset.Y< 0d)
                        //{
                        //    bezierviewModel.StartPoint = _connectionInformation.StartPoint;
                        //    bezierviewModel.Point1 = new(_connectionInformation.StartPoint.X + 50, _connectionInformation.StartPoint.Y);
                        //    bezierviewModel.Point2 = new(currentPoint.X - 50, currentPoint.Y);
                        //    bezierviewModel.Point3 = currentPoint;
                        //}
                    }

                    DoPolyLineSegment();
                    void DoPolyLineSegment()
                    {
                        var points = CreatePolyPoints(_connectionInformation.StartPoint, currentPoint);
                        var polyLineSegmentViewModel = _connectionInformation.ShapeElementViewModel as PolyLineSegmentViewModel;
                        //  polyLineSegmentViewModel.Stretch = Stretch.Fill;
                        //polyLineSegmentViewModel.Stroke = Brushes.MediumPurple;
                        //polyLineSegmentViewModel.StrokeThickness = 3d;
                        //polyLineSegmentViewModel.StrokeDashCap = PenLineCap.Flat;
                        polyLineSegmentViewModel.StartPoint = _connectionInformation.StartPoint;
                        polyLineSegmentViewModel.Points = points;
                    }

                    DoDragDropItem();
                    void DoDragDropItem()
                    {
                        double moveX = Math.Min(currentPoint.X, _connectionInformation.StartPoint.X);
                        double moveY = Math.Min(currentPoint.Y, _connectionInformation.StartPoint.Y);
                        double moveWidth = Math.Abs(currentPoint.X - _connectionInformation.StartPoint.X);
                        double moveHeight = Math.Abs(currentPoint.Y - _connectionInformation.StartPoint.Y);
                        _connectionInformation.DragDropItemViewModel.X = moveX;
                        _connectionInformation.DragDropItemViewModel.Y = moveY;
                        _connectionInformation.DragDropItemViewModel.Width = moveWidth;
                        _connectionInformation.DragDropItemViewModel.Height = moveHeight;
                        _connectionInformation.ShapeElement.Width = moveWidth;
                        _connectionInformation.ShapeElement.Height = moveHeight;
                    }

                    // Debug.Print($"MouseMove.Canvas:X={endPoint.X} Y={endPoint.Y}");
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

        #region CreateSegment Method
        private PathSegment CreateSegment(string type, Point startPoint, Point endPoint)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new Exception("type 类型不能为空");
            }

            PathSegment segment;
            if (type == "polyline")
            {
                if (startPoint.X <= endPoint.X - 40) // 两边距离大于40
                {
                    CreatePolyline();
                }
                else
                {
                    CreatePolyLineSegment();
                }
            }
            else
            {
                CreateBezierSegment();
            }

            void CreatePolyline()
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

            void CreatePolyLineSegment()
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

            void CreateBezierSegment()
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

        private PointCollection CreatePolyPoints(Point startPoint, Point endPoint)
        {
            if (startPoint.X <= endPoint.X - 40) // 两边距离大于40
            {
                CreateTwoPolyline();
            }
            else
            {
                CreateThreePolyLineSegment();
            }

            PointCollection pointCollection;
            void CreateTwoPolyline()
            {
                double centerX = (startPoint.X + endPoint.X) / 2;

                pointCollection = new()
                {
                    new Point(centerX,startPoint.Y),
                    new Point(centerX,endPoint.Y),
                    new Point(endPoint.X,endPoint.Y)    // 终点
                      
               };
            }

            void CreateThreePolyLineSegment()
            {
                double centerY = (startPoint.Y + endPoint.Y) / 2;

                pointCollection = new()
                {
                    new Point(startPoint.X + 20,startPoint.Y),
                    new Point(startPoint.X + 20,centerY),
                    new Point(endPoint.X - 20,centerY),
                    new Point(endPoint.X - 20,endPoint.Y),
                    new Point(endPoint.X,endPoint.Y)    // 终点    
                };
            }

            return pointCollection;
        }
        #endregion

        #region ItemsControl MouseLeftButtonUp Event
        public void ExecutePreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            //Debug.Print($"DragDropView:MouseLeftButtonUp");

            if (IsDragging())
            {
                return;
            }

            Canvas mainCanvas;

            if (IsConnection())
            {
                if (e.Source is ItemsControl itemsControl)
                {
                    VisualTreeFinder visualTreeFinder = new();

                    mainCanvas = visualTreeFinder.FindVisualChild<Canvas>(itemsControl);
                    var currentPoint = Mouse.GetPosition(mainCanvas);

                    var nodeView = visualTreeFinder.FindVisualParent<XNodeView>(_connectionInformation.OutputPort);
                    var inputNodeRef = visualTreeFinder.FindVisualChilds<Border>(nodeView).FirstOrDefault(d => (d is Border) && (d as Border).Name == "InputNode");

                    FrameworkElement nearestInputPort = null;
                    double minDist = double.MaxValue;

                    #region NearestInputPort Method
                    foreach (var inputPort in InputPorts)
                    {
                        if (inputPort == inputNodeRef)
                        {
                            continue;
                        }

                        Point portCenter = GetInputPortCenter(inputPort);

                        double dist = ((Point)portCenter - currentPoint).Length;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearestInputPort = inputPort;
                        }
                    }

                    if (nearestInputPort != null && minDist < 10) // 连线和接口的可吸附距离
                    {
                        var endPoint = GetInputPortCenter(nearestInputPort);

                        //DoBezier();
                        void DoBezier()
                        {
                            var bezierviewModel = _connectionInformation.ShapeElementViewModel as BezierViewModel;
                            //bezierviewModel.Stretch = Stretch.Fill;
                            //bezierviewModel.Stroke = Brushes.MediumPurple;
                            //bezierviewModel.StrokeThickness = 3d;
                            bezierviewModel.StartPoint = _connectionInformation.StartPoint;
                            bezierviewModel.Point1 = new Point(_connectionInformation.StartPoint.X + 50, _connectionInformation.StartPoint.Y);
                            bezierviewModel.Point2 = new Point(endPoint.X - 50, endPoint.Y);
                            bezierviewModel.Point3 = endPoint;
                        }

                        DoPolyLineSegment();
                        void DoPolyLineSegment()
                        {
                            var points = CreatePolyPoints(_connectionInformation.StartPoint, endPoint);
                            var polyLineSegmentViewModel = _connectionInformation.ShapeElementViewModel as PolyLineSegmentViewModel;
                            //polyLineSegmentViewModel.Stretch = Stretch.Fill;
                            //polyLineSegmentViewModel.Stroke = Brushes.MediumPurple;
                            //polyLineSegmentViewModel.StrokeThickness = 3d;
                            //polyLineSegmentViewModel.StrokeDashCap = PenLineCap.Flat;
                            polyLineSegmentViewModel.StartPoint = _connectionInformation.StartPoint;
                            polyLineSegmentViewModel.Points = points;
                        }

                        DoDragDropItem();
                        void DoDragDropItem()
                        {
                            double moveX = Math.Min(currentPoint.X, _connectionInformation.StartPoint.X);
                            double moveY = Math.Min(currentPoint.Y, _connectionInformation.StartPoint.Y);
                            double moveWidth = Math.Abs(endPoint.X - _connectionInformation.StartPoint.X);
                            double moveHeight = Math.Abs(endPoint.Y - _connectionInformation.StartPoint.Y);
                            _connectionInformation.DragDropItemViewModel.X = moveX;
                            _connectionInformation.DragDropItemViewModel.Y = moveY;
                            _connectionInformation.DragDropItemViewModel.Width = moveWidth;
                            _connectionInformation.DragDropItemViewModel.Height = moveHeight;
                            _connectionInformation.ShapeElement.Width = moveWidth;
                            _connectionInformation.ShapeElement.Height = moveHeight;
                        }

                        Connections.Add(new Connection
                        {
                            FromPort = _connectionInformation.OutputPort,
                            ToPort = nearestInputPort,
                            DragDropItemViewModel = _connectionInformation.DragDropItemViewModel,
                            ShapeElement = _connectionInformation.ShapeElement,
                            ShapeElementViewModel = _connectionInformation.ShapeElementViewModel,
                        });
                    }
                    else
                    {
                        // 拖空则移除
                        DragDropItems.Remove(_connectionInformation.DragDropItemViewModel);
                    }
                    #endregion

                    #region HitTes Method
                    //DependencyObject hitObject = mainCanvas.InputHitTest(currentPoint) as DependencyObject;
                    //var hitObject1 = VisualTreeHelper.HitTest(mainCanvas, currentPoint);

                    //var borders = visualTreeFinder.FindVisualChilds<Border>(mainCanvas);
                    //var inputPortBorder = borders.FirstOrDefault(d => d.Name == "InputNode" && hitObject == d);

                    //if (inputPortBorder is not  null)
                    //{
                    //    var endPoint = GetInputPortCenter(inputPortBorder);

                    //    DoBezier();
                    //    void DoBezier()
                    //    {
                    //        var bezierviewModel = _connectionInformation.CurrentViewModel as BezierViewModel;
                    //        //bezierviewModel.Stretch = Stretch.Fill;
                    //        //bezierviewModel.Stroke = Brushes.MediumPurple;
                    //        //bezierviewModel.StrokeThickness = 3d;
                    //        bezierviewModel.StartPoint = _connectionInformation.StartPoint;
                    //        bezierviewModel.Point1 = new Point(_connectionInformation.StartPoint.X + 50, _connectionInformation.StartPoint.Y);
                    //        bezierviewModel.Point2 = new Point(endPoint.X - 50, endPoint.Y);
                    //        bezierviewModel.Point3 = endPoint;
                    //    }

                    //   // DoPolyLineSegment();
                    //    void DoPolyLineSegment()
                    //    {
                    //        var points = CreatePolyPoints(_connectionInformation.StartPoint, endPoint);
                    //        var polyLineSegmentViewModel = _connectionInformation.CurrentViewModel as PolyLineSegmentViewModel;
                    //        //polyLineSegmentViewModel.Stretch = Stretch.Fill;
                    //        //polyLineSegmentViewModel.Stroke = Brushes.MediumPurple;
                    //        //polyLineSegmentViewModel.StrokeThickness = 3d;
                    //        //polyLineSegmentViewModel.StrokeDashCap = PenLineCap.Flat;
                    //        polyLineSegmentViewModel.StartPoint = _connectionInformation.StartPoint;
                    //        polyLineSegmentViewModel.Points = points;
                    //    }

                    //    DoDragDropItem();
                    //    void DoDragDropItem()
                    //    {
                    //        double moveX = Math.Min(currentPoint.X, _connectionInformation.StartPoint.X);
                    //        double moveY = Math.Min(currentPoint.Y, _connectionInformation.StartPoint.Y);
                    //        double moveWidth = Math.Abs(endPoint.X - _connectionInformation.StartPoint.X);
                    //        double moveHeight = Math.Abs(endPoint.Y - _connectionInformation.StartPoint.Y);
                    //        _connectionInformation.CurrentDragDropItemViewModel.X = moveX;
                    //        _connectionInformation.CurrentDragDropItemViewModel.Y = moveY;
                    //        _connectionInformation.CurrentDragDropItemViewModel.Width = moveWidth;
                    //        _connectionInformation.CurrentDragDropItemViewModel.Height = moveHeight;
                    //        _connectionInformation.CurrentViewElement.Width = moveWidth;
                    //        _connectionInformation.CurrentViewElement.Height = moveHeight;
                    //    }
                    //}
                    //else
                    //{
                    //    // 拖空则移除
                    //    DragDropItems.Remove(_connectionInformation.CurrentDragDropItemViewModel);
                    //}
                    #endregion

                    _connectionInformation.IsConnecting = false;
                    _connectionInformation.DragDropItemViewModel = null;
                    _connectionInformation.ShapeElement = null;
                    _connectionInformation.ShapeElementViewModel = null;
                    //_connectionInformation.CurrentPath = null;
                }

                System.Windows.Point GetInputPortCenter(FrameworkElement inputPortBorder)
                {
                    var centerPoint = new System.Windows.Point(inputPortBorder.Width / 2, inputPortBorder.Height / 2);
                    var pointRelativeTo = inputPortBorder.TranslatePoint(centerPoint, mainCanvas);
                    return pointRelativeTo;
                }
            }

            bool IsDragging() => (_selectedDragDropItem is not null) && _selectedDragDropItem.IsDown;

            bool IsConnection() => _connectionInformation.IsConnecting && _connectionInformation.DragDropItemViewModel is not null && _connectionInformation.ShapeElement is not null;

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

        #region PolyLineSegmentView Mouse Events
        private FrameworkElement _currentViewElement;
        private void PolyLineMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is PolyLineSegmentView polyLineSegmentView)
            {
                _currentViewElement = polyLineSegmentView;
            }
        }

        private void PolyLineMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is PolyLineSegmentView polyLineSegmentView)
            {
                var polyLineSegmentViewModel = polyLineSegmentView?.DataContext as PolyLineSegmentViewModel;

                polyLineSegmentViewModel.Stroke = Brushes.Orange;
                polyLineSegmentViewModel.StrokeThickness = 3d;

                polyLineSegmentView.Cursor = Cursors.Hand;
            }
        }

        private void PolyLineMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is PolyLineSegmentView polyLineSegmentView)
            {
                var polyLineSegmentViewModel = polyLineSegmentView?.DataContext as PolyLineSegmentViewModel;

                polyLineSegmentViewModel.Stroke = Brushes.MediumPurple;
                polyLineSegmentViewModel.StrokeThickness = 3d;
            }
        }
        #endregion

        #region Loaded Event
        public void ExecuteLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.UserControl uc)
            {
                try
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var windows = visualTreeFinder.FindVisualParents<Window>(uc);
                    var shell = windows.FirstOrDefault(w => w.Name == "WindowOfShell");

                    if (shell is not null)
                    {
                        shell.KeyDown += async (sender, e) =>
                        {
                            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                            {
                                //var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                                //bool? flag = saveFileDialog.ShowDialog();
                                //if (flag == null || flag.Value == false) return;
                                //string filePath = saveFileDialog.FileName;
                                //   SaveCanvasAsImage(MainCanvas, filePath);

                                await Connections.ToAdjacencyListAsync();
                            }
                            else if (e.Key == Key.Delete && _currentViewElement is not null)
                            {
                                Connections.RemoveAll(conn => conn.ShapeElement == _currentViewElement);

                                VisualTreeFinder visualTreeFinder = new();
                                var dragDropItemViewToShape = visualTreeFinder.FindVisualParent<DragDropItemView>(_currentViewElement);
                                var dragDropItemViewModelToShape = dragDropItemViewToShape.DataContext as DragDropItemViewModel;
                                DragDropItems.Remove(dragDropItemViewModelToShape);
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                     _dialogViewService.AlertAsync(message: $"Loaded Error: \"{ex.Message}\"", title: "Error").Await() ;
                }
            }
        }
        #endregion

    }
}
