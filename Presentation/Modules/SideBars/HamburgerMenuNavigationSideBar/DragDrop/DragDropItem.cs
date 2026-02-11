using System;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class DragDropItem
    {
        #region Constructors
        public DragDropItem()
        {

        }
        #endregion

        #region Properties
        public double X { get; set; }

        public double Y { get; set; }
       
        public double Width { get; set; }

        public double Height { get; set; }

        public string ViewName { get; set; }
        #endregion
    }
}
