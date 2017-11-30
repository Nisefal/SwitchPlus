using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace My_custom_switch
{
    public sealed partial class MySwitchControl : UserControl
    {
        public MySwitchControl()
        {
            this.InitializeComponent();
            Rectangle back = new Rectangle();
            double _height = 40;
            double _width = Draw.RenderSize.Width;
            back.RadiusX = _height / 2;
            back.RadiusY = _height / 2;
            back.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            if (_height < _width)
            {
                back.Height = _height;
                back.Width = _width;
            }
            Draw.Children.Add(back);
        }

        private Ball _Ball
        {
            get { return (Ball)GetValue(_BallProperty); }
            set { SetValue(_BallProperty, value); }
        }

        // Using a DependencyProperty as the backing store for _Ball.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty _BallProperty =
            DependencyProperty.Register("_Ball", typeof(Ball), typeof(MySwitchControl), new PropertyMetadata(0));

        [System.ComponentModel.DefaultValue(0)]
        public new int Height;
        [System.ComponentModel.DefaultValue(0)]
        public new int Width;
        [System.ComponentModel.DefaultValue(false)]
        public bool mode;

        public class Ball : Canvas
        {
            private int Radius;
            private int OuterRadius;
            private int x;
            private int y;

            public Ball(int rad_out, int _x, int _y)
            {
                OuterRadius = rad_out;
                Radius = rad_out - 2;
                x = _x;
                y = _y;
            }

            public void setX(int _x)
            {
                x = _x;
            }

            public void setY(int _y)
            {
                y = _y;
            }

            public void setCoordinates(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public int getX()
            {
                return x;
            }

            public int getY()
            {
                return y;
            }
        }

        private void Draw_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Rectangle back = new Rectangle();
            double _height = 40;
            double _width = Draw.RenderSize.Width;
            back.RadiusX = _height / 2;
            back.RadiusY = _height / 2;
            back.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            if (_height < _width) // && _width && _height)
            {
                back.Height = _height;
                back.Width = _width;
            }
            else
            {
                back.Width = ((Rectangle)Draw.Children[0]).Width;
            }
            Draw.Children.Clear();
            Draw.Children.Add(back);
        }
    }
}
