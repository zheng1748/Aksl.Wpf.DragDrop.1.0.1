using Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels;
using Aksl.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar
{
    public class ConnectionInformation
    {
        #region Constructors
        public ConnectionInformation()
        {
            Connections = new();
            InputPorts = new();
        }
        #endregion

        #region Properties
        public bool IsConnecting { get; set; }
        public Point StartPoint { get; set; }
        public Border OutputPortRef { get; set; }
        public Border InputNodeRef { get; set; }
        //public System.Windows.Shapes.Path CurrentPath { get; set; }
        // public DragDropItemViewModel DragDropItemViewModelToBezier { get; set; }
        public DragDropItemViewModel CurrentDragDropItemViewModel{ get; set; }
        public FrameworkElement CurrentViewElement { get; set; }
        public BindableBase CurrentViewModel { get; set; }
        //public BezierViewModel BezierViewModel { get; set; }
        //public PathViewModel PathViewModel { get; set; }
        //public PolyLineSegmentViewModel PolyLineSegmentViewModel { get; set; }
        public List<Connection> Connections { get; set; }
        public List<Border> InputPorts { get; set; }
        #endregion
    }

    public class Connection
    {
        public FrameworkElement FromPort { get; set; }
        public FrameworkElement ToPort { get; set; }
        public System.Windows.Shapes.Path Path { get; set; }
        public DragDropItemViewModel DragDropItemViewModel { get; set; }
        public FrameworkElement ViewElement { get; set; }
    }
}
