using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace My_custom_switch
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Do();
        }

        private void Do()
        {
            Ellipse left = new Ellipse();
            Ellipse right = new Ellipse();
            Rectangle middle = new Rectangle();
            left.Height = Canvas1.RenderSize.Height;
            left.Width = Canvas1.RenderSize.Height;
            left.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            left.Margin = new Thickness(0, 0, 0, 0);
            right.Height = Canvas1.RenderSize.Height;
            right.Width = Canvas1.RenderSize.Height;
            right.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            right.Margin = new Thickness(Canvas1.RenderSize.Width - Canvas1.RenderSize.Height, 0, 0, 0);
            middle.Height = Canvas1.RenderSize.Height;
            middle.Width = Canvas1.RenderSize.Width - Canvas1.RenderSize.Height;
            middle.Margin = new Thickness(Canvas1.RenderSize.Height / 2, 0, 0, 0);
            middle.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
            Canvas1.Children.Add(left);
            Canvas1.Children.Add(right);
            Canvas1.Children.Add(middle);
        }

        private void Canvas1_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
