using Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels;
using Aksl.Modules.HamburgerMenuNavigationSideBar.Views;
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
            // Connections = new();
            //InputPorts = new();
        }
        #endregion

        #region Properties
        public bool IsConnecting { get; set; }
        public Point StartPoint { get; set; }
        public Border OutputPort { get; set; }
        //public Border InputNodeRef { get; set; }
        //public System.Windows.Shapes.Path CurrentPath { get; set; }
        // public DragDropItemViewModel DragDropItemViewModelToBezier { get; set; }
     //   public DragDropItemView DragDropItemView { get; set; }
        public DragDropItemViewModel DragDropItemViewModel { get; set; }
        public FrameworkElement ShapeElement { get; set; }
        public BindableBase ShapeElementViewModel { get; set; }
        //public BezierViewModel BezierViewModel { get; set; }
        //public PathViewModel PathViewModel { get; set; }
        //public PolyLineSegmentViewModel PolyLineSegmentViewModel { get; set; }
        //  public List<Connection> Connections { get; set; }

        #endregion
    }

    public class Connection
    {
       // public DragDropItemView DragDropItemView { get; set; }
        public FrameworkElement FromPort { get; set; }
        public FrameworkElement ToPort { get; set; }
        public DragDropItemViewModel DragDropItemViewModel { get; set; }
        public FrameworkElement ShapeElement { get; set; }
        public BindableBase ShapeElementViewModel { get; set; }
    }
}
