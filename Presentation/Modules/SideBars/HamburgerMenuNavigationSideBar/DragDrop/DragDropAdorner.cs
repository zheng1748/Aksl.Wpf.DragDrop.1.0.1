using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar
{
    public class DragDropAdorner : Adorner
    {
        #region Members
        private FrameworkElement _draggedElement = null;
        #endregion

        #region Constructors
        public DragDropAdorner(UIElement parent)
            : base(parent)
        {
            IsHitTestVisible = false; // Seems Adorner is hit test visible?
            _draggedElement = parent as FrameworkElement;
        }
        #endregion

        #region OnRender Method
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_draggedElement is not null)
            {
                Win32.POINT screenPos = new();
                if (Win32.GetCursorPos(ref screenPos))
                {
                    Point pos = this.PointFromScreen(new Point(screenPos.X, screenPos.Y));
                    Rect rect = new Rect(pos.X, pos.Y, _draggedElement.ActualWidth, _draggedElement.ActualHeight);
                    drawingContext.PushOpacity(1.0);
                    Brush highlight = _draggedElement.TryFindResource(SystemColors.HighlightBrushKey) as Brush;
                    if (highlight is not null)
                    {
                        drawingContext.DrawRectangle(highlight, new Pen(Brushes.Transparent, 0), rect);
                    }

                    drawingContext.DrawRectangle(new VisualBrush(_draggedElement), new Pen(Brushes.Transparent, 0), rect);
                }
            }
        }
    }
    #endregion

    #region Win32
    public static class Win32
    {
        public struct POINT { public Int32 X; public Int32 Y; }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT point);
    }
    #endregion
}
