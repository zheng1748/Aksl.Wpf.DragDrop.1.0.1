using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Aksl.Views
{
    public partial class XNodeView : UserControl
    {
        public XNodeView()
        {
            InitializeComponent();
        }

        #region Dependency Properties
        public Brush HeaderBackgroundColor
        {
            get => (Brush)GetValue(HeaderBackgroundColorProperty);
            set => SetValue(HeaderBackgroundColorProperty, value);
        }

        public static readonly DependencyProperty HeaderBackgroundColorProperty =
            DependencyProperty.Register("HeaderBackgroundColor", typeof(Brush), typeof(XNodeView), new PropertyMetadata(defaultValue: new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8")), propertyChangedCallback: null));

        public Brush HeaderForegroundColor
        {
            get => (Brush)GetValue(HeaderForegroundColorProperty);
            set => SetValue(HeaderForegroundColorProperty, value);
        }

        public static readonly DependencyProperty HeaderForegroundColorProperty =
            DependencyProperty.Register("HeaderForegroundColor", typeof(Brush), typeof(XNodeView), new PropertyMetadata(defaultValue: new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), propertyChangedCallback: null));

        public Brush ContentBackgroundColor
        {
            get => (Brush)GetValue(ContentBackgroundColorProperty);
            set => SetValue(ContentBackgroundColorProperty, value);
        }

        public static readonly DependencyProperty ContentBackgroundColorProperty =
            DependencyProperty.Register("ContentBackgroundColor", typeof(Brush), typeof(XNodeView), new PropertyMetadata(defaultValue: new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), propertyChangedCallback: null));

        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(XNodeView), new PropertyMetadata(defaultValue: "节点信息", propertyChangedCallback: null));

        public double LineWidth
        {
            get => (double)GetValue(LineWidthProperty);
            set => SetValue(LineWidthProperty, value);
        }

        public static readonly DependencyProperty LineWidthProperty =
            DependencyProperty.Register("LineWidth", typeof(double), typeof(XNodeView), new PropertyMetadata(defaultValue: 3d, propertyChangedCallback: null));

        public double Shrink
        {
            get => (double)GetValue(ShrinkProperty);
            set => SetValue(ShrinkProperty, value);
        }

        public static readonly DependencyProperty ShrinkProperty =
            DependencyProperty.Register("Shrink", typeof(double), typeof(XNodeView), new PropertyMetadata(defaultValue: 8d, propertyChangedCallback: null));

        public Visibility BorderVisible
        {
            get => (Visibility)GetValue(BorderVisibleProperty);
            set => SetValue(BorderVisibleProperty, value);
        }

        public static readonly DependencyProperty BorderVisibleProperty =
            DependencyProperty.Register("BorderVisible", typeof(Visibility), typeof(XNodeView), new PropertyMetadata(defaultValue: Visibility.Visible, propertyChangedCallback: null));

        public  bool Focused
        {
            get => (bool)GetValue(FocusedProperty);
            set => SetValue(FocusedProperty, value);
        }

        public static readonly DependencyProperty FocusedProperty =
            DependencyProperty.Register("Focused", typeof(bool), typeof(XNodeView), new PropertyMetadata(defaultValue: false, propertyChangedCallback: null));

        private static void OnFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is XNodeView nodeView)
            {
                if ((bool)e.NewValue != (bool)e.OldValue)
                {
                    nodeView.BorderVisible= (bool)e.NewValue ? Visibility.Visible: Visibility.Collapsed;
                }
            }
        }
        #endregion
    }
}
