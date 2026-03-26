using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Aksl.Toolkit.UI;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Aksl.ViewModels
{
    public class PathViewModel : BindableBase
    {
        #region Members
        #endregion

        #region Constructors
        public PathViewModel()
        {
        }
        #endregion

        #region Properties
        private Stretch _stretch = Stretch.Fill;
        public Stretch Stretch
        {
            get => _stretch;
            set => SetProperty<Stretch>(ref _stretch, value);
        }

        private Brush _stroke = Brushes.MediumPurple;
        public Brush Stroke
        {
            get => _stroke;
            set => SetProperty<Brush>(ref _stroke, value);
        }

        private double _strokeThickness = 3d;
        public double StrokeThickness
        {
            get => _strokeThickness;
            set => SetProperty<double>(ref _strokeThickness, value);
        }

        public List<PenLineCap> PenLineCapList
        {
            get => Enum.GetValues(typeof(PenLineCap)).Cast<PenLineCap>().ToList();
        }

        private PenLineCap _strokeDashCap = PenLineCap.Flat;
        public PenLineCap StrokeDashCap
        {
            get => _strokeDashCap;
            set => SetProperty<PenLineCap>(ref _strokeDashCap, value);
        }

        private Point _startPoint;
        public Point StartPoint
        {
            get => _startPoint;
            set => SetProperty<Point>(ref _startPoint, value);
        }

        private Geometry _data;
        public Geometry Data
        {
            get => _data;
            set => SetProperty<Geometry>(ref _data, value);
        }

        private ContextMenu _popupMenu;
        public ContextMenu PopupMenu
        {
            get => _popupMenu;
            set => SetProperty(ref _popupMenu, value);
        }
        #endregion
    }
}
