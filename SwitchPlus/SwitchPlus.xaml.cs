using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SwitchPlus
{
    public sealed partial class SwitchPlus : UserControl
    {
        private bool draged;
        private double position;

        private double delta;
        private double overal_delta;
        private Rectangle back = new Rectangle();
        private Rectangle fill = new Rectangle();
        private Rectangle shade = new Rectangle();
        private Rectangle knob = new Rectangle();
        private Rectangle main_shade = new Rectangle();
        private double _height = 400;
        private double _width = 1000;
        private int shade_delta = 20;
        private double _old_position;
        private double _additional_width_r, _additional_width_l;
        private bool tmpState;
        private DispatcherTimer timer;

        public delegate void MyEventHandler(object sender, EventArgs args);
        public event MyEventHandler OnStateChanged;
        public event MyEventHandler OnValueChanged;

        private bool _isBeingDragged
        {
            get { return draged; }
            set
            {
                draged = value;
                LongSwitchState();
            }
        }

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background",
                                             typeof(Color), typeof(SwitchPlus), new PropertyMetadata(Colors.Orange));
        public new Windows.UI.Color Background
        {
            set
            {
                SetValue(BackgroundProperty, value);
            }
            get
            {
                return (Color)GetValue(BackgroundProperty);
            }
        }

        public static readonly DependencyProperty KnobPaddingProperty = DependencyProperty.Register("KnobPadding",
                                                     typeof(int), typeof(SwitchPlus), new PropertyMetadata(0));

        public int KnobPadding
        {
            get { return (int)GetValue(KnobPaddingProperty); }
            set
            {
                if (value <= 100)
                    SetValue(KnobPaddingProperty, value);
                else
                    SetValue(KnobPaddingProperty, 0);
            }
        }

        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register("IsOn",
                                                    typeof(bool), typeof(SwitchPlus), new PropertyMetadata(false));

        public bool IsOn
        {
            get
            {
                return (bool)GetValue(IsOnProperty);
            }
            set
            {
                SetValue(IsOnProperty, value);
                if (value != (bool)GetValue(IsOnProperty))
                {
                    OnStateChanged(this, new EventArgs());
                }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                    typeof(string), typeof(SwitchPlus), new PropertyMetadata("Switch"));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SwitchPlus()
        {
            this.InitializeComponent();
            this.Loaded += SwitchPlus_Loaded;
        }

        private void SwitchPlus_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchInitState();
            this.Loaded -= SwitchPlus_Loaded;
        }

        private void SwitchInitState()
        {
            back.Height = _height;
            back.Width = _width;
            fill.Height = _height;
            fill.Width = _width;
            shade.Height = _height - KnobPadding * 2;
            shade.Width = _height - KnobPadding * 2;
            knob.Height = _height - KnobPadding * 2;
            knob.Width = _height - KnobPadding * 2;
            main_shade.Height = _height;
            main_shade.Width = _height;

            back.RadiusX = _height / 2;
            back.RadiusY = _height / 2;
            fill.RadiusX = _height / 2;
            fill.RadiusY = _height / 2;
            shade.RadiusX = shade.Height / 2;
            shade.RadiusY = shade.Height / 2;
            knob.RadiusX = knob.Height / 2;
            knob.RadiusY = knob.Height / 2;
            main_shade.RadiusX = main_shade.Height / 2;
            main_shade.RadiusY = main_shade.Height / 2;
            main_shade.Height = _height;
            main_shade.Width = _height;

            shade.Opacity = 0.5;
            main_shade.Opacity = 0.3;
            fill.Opacity = 0;

            if (IsOn)
            {
                main_shade.Margin = new Thickness(_width - _height, 0, 0, 0);
                shade.Margin = new Thickness(_width - _height + KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(_width - _height + KnobPadding, KnobPadding, 0, 0);
            }
            else
            {
                main_shade.Margin = new Thickness(0, 0, 0, 0);
                shade.Margin = new Thickness(KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
            }

            main_shade.Fill = new SolidColorBrush(Colors.Black);
            shade.Fill = new SolidColorBrush(Colors.Black);
            knob.Fill = new SolidColorBrush(Colors.White);
            fill.Fill = new SolidColorBrush(Colors.White);
            back.Fill = new SolidColorBrush(Background);

            C1.Children.Add(back);
            C2.Children.Add(fill);
            C2.Children.Add(main_shade);
            C2.Children.Add(shade);
            C2.Children.Add(knob);
            SetTimerRollBack();
            main_shade.HorizontalAlignment = HorizontalAlignment.Right;
            shade.HorizontalAlignment = HorizontalAlignment.Right;
            knob.HorizontalAlignment = HorizontalAlignment.Right;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SwitchState()
        {
            if (IsOn)
            {
                main_shade.Margin = new Thickness(_width - _height, 0, 0, 0);
                shade.Margin = new Thickness(_width - _height + KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(_width - _height + KnobPadding, KnobPadding, 0, 0);
            }
            else
            {
                main_shade.Margin = new Thickness(0, 0, 0, 0);
                shade.Margin = new Thickness(KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
            }
        }

        private void LongSwitchState()
        {

        }

        private void C2_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is Rectangle && !e.OriginalSource.Equals(C2.Children[0]))
            {
                _isBeingDragged = true;
            }
            tmpState = IsOn;
            timer.Start();
        }

        private void C2_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isBeingDragged = false;
            delta = 0;
            _old_position = 0;
            position = 0;

            IsOn = tmpState;
            LongSwitchState();
        }

        private void Canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsOn)
                IsOn = false;
            else
                IsOn = true;
            SwitchState();
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isBeingDragged)
            {
                Windows.UI.Input.PointerPoint pp = e.GetCurrentPoint(relativeTo: this.C2);
                position = pp.Position.X;

                if (_old_position != 0)
                    delta = position - _old_position;
                else
                    delta = 0;

                if (OnValueChanged != null)
                    OnValueChanged(this, new EventArgs());
                overal_delta += delta;
                if (overal_delta <= _width - _height && overal_delta >= _height / 2)
                {
                    if (delta < 0)
                    {
                        if (_additional_width_r != 0)
                        {
                            _additional_width_r += delta;
                            if (_additional_width_r < 0)
                                _additional_width_r = 0;
                        }
                        else
                        {
                            _additional_width_l -= delta;
                        }
                    }
                    else
                    {
                        if (_additional_width_l != 0)
                        {
                            _additional_width_l -= delta;
                            if (_additional_width_l < 0)
                                _additional_width_l = 0;
                        }
                        else
                        {
                            _additional_width_r += delta;
                        }
                    }
                }

                double percentage = (2 * position - _height) / _width; // checks if center of knob surpasses 1/2 of the control
                if (percentage >= 0.5)
                {
                    tmpState = true;
                }
                else
                {
                    tmpState = false;
                }

                _old_position = position;
            }
        }
        private void SetTimerRollBack()
        {
            timer = new DispatcherTimer();
            timer.Tick += RollBack;
            timer.Interval = new TimeSpan(0,0,0,0,5);
        }

        private void RollBack(object sender, object e)
        {
            _additional_width_r -= 40;
            if (_additional_width_r <= 0)
                _additional_width_r = 0;
            _additional_width_l -= 40;
            if (_additional_width_l <= 0)
                _additional_width_l = 0;
            double _additional_width = _additional_width_l + _additional_width_r;

            if (delta > 0)
            {
                main_shade.Width = _height + _additional_width;
                shade.Width = _height + _additional_width - KnobPadding * 2;
                knob.Width = _height + _additional_width - KnobPadding * 2;
                double res = overal_delta - _height/2 - _additional_width;
                if (overal_delta  >= _width - _height)
                {
                    main_shade.Margin = new Thickness(_width - _height - _additional_width, 0, 0, 0);
                    shade.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding + shade_delta, 0, 0);
                    knob.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                }
                else
                {
                    if (overal_delta <= _height / 2)
                    {
                        main_shade.Margin = new Thickness(0, 0, 0, 0);
                        shade.Margin = new Thickness(KnobPadding, KnobPadding + shade_delta, 0, 0);
                        knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                    }
                    else
                    {
                    main_shade.Margin = new Thickness(res, 0, 0, 0);
                    shade.Margin = new Thickness(res + KnobPadding, KnobPadding + shade_delta, 0, 0);
                    knob.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                    }
                }
            }
            else
            {
                main_shade.Width = _height + _additional_width;
                shade.Width = _height + _additional_width - KnobPadding * 2;
                knob.Width = _height + _additional_width - KnobPadding * 2;
                double res = overal_delta - _height/2;
                if(overal_delta <= _height/2)
                {
                    main_shade.Margin = new Thickness(0, 0, 0, 0);
                    shade.Margin = new Thickness(KnobPadding, KnobPadding + shade_delta, 0, 0);
                    knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                }
                else
                {
                    if (overal_delta >= _width - _height)
                    {
                        main_shade.Margin = new Thickness(_width - _height - _additional_width, 0, 0, 0);
                        shade.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding + shade_delta, 0, 0);
                        knob.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                    }
                    else
                    {
                        main_shade.Margin = new Thickness(res, 0, 0, 0);
                        shade.Margin = new Thickness(res + KnobPadding, KnobPadding + shade_delta, 0, 0);
                        knob.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                    }
                }
            }
            if (!_isBeingDragged && _additional_width == 0)
                timer.Stop();
        }
    }
}