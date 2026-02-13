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

        public System.Windows.Shapes.Path CurrentPath { get; set; }

        public FrameworkElement FromPort { get; set; }
        public List<Connection> Connections { get; set; }

        public List<FrameworkElement> InputPorts { get; set; }
        #endregion
    }

    public class Connection
    {
        public FrameworkElement FromPort { get; set; }
        public FrameworkElement ToPort { get; set; }
        public System.Windows.Shapes.Path Path { get; set; }
    }
}
