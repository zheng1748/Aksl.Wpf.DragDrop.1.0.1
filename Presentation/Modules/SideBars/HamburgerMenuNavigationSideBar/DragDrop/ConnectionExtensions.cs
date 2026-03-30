using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Aksl.Toolkit.UI;
using Aksl.Views;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar
{
    public static class ConnectionExtensions
    {
        public static async Task ToAdjacencyListAsync(this List<Connection> connectionList)
        {
            VisualTreeFinder visualTreeFinder = new();
            IDictionary<XNodeView, IList<XNodeView>> adjacencyList = new Dictionary<XNodeView, IList<XNodeView>>();

            foreach (var conn in connectionList)
            {
                var fromNodeView = visualTreeFinder.FindVisualParent<XNodeView>(conn.FromPort);

                AddToAdjacencyList(fromNodeView);

                //var refConns = connectionList.Where(c => IsChildOf(c.FromPort, fromNodeView)).ToList();

                //List< XNodeView> allTravelNodeViews = new();
            }

            foreach (var nodeView in adjacencyList.Keys)
            {
                var refConns = connectionList.Where(c => IsChildOf(c.FromPort, nodeView)).ToList();

               var toNodeViews = refConns.Select(c => FindNodeViewByPort(c.ToPort)).ToList();

                AddRange(toNodeViews, nodeView);
            }

            void GetAdjacencies(XNodeView nodeView, IList<XNodeView> travelNodeViews)
            {
                if (!IsTraveled(travelNodeViews, nodeView))
                {
                    travelNodeViews.Add(nodeView);
                }
            }

            bool IsTraveled(IEnumerable<XNodeView> nodeViews, XNodeView nodeView)
            {
                return nodeViews.Contains(nodeView);
            }

            XNodeView FindNodeViewByPort(FrameworkElement port)
            {
                var nodeView = visualTreeFinder.FindVisualParent<XNodeView>(port);
                return nodeView;
            }

            void AddToAdjacencyList(XNodeView xNodeView)
            {
                if (!adjacencyList.ContainsKey(xNodeView))
                {
                    adjacencyList.Add(xNodeView,new List<XNodeView>());
                }
            }

            void AddRange(IEnumerable< XNodeView> nodeViews,XNodeView nodeView)
            {
                if (adjacencyList.ContainsKey(nodeView))
                {
                    foreach (var nv in nodeViews)
                    {
                        adjacencyList[nodeView].Add(nv);
                    };
                }
            }

            bool IsChildOf(FrameworkElement child, DependencyObject parent)
            {
                var childs = visualTreeFinder.FindVisualChilds<DependencyObject>(parent);
                return childs.Contains(child);
            }
        }
    }
}
