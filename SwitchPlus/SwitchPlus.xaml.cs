using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Input;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SwitchPlus
{
    public sealed partial class SwitchPlus : UserControl
    {
        private double position;        // pointer position X
        private bool drag;              // indicates if there was dradding of knob or not
        private bool mod;               // indicates of situation when direction of move was rapidly changed (to decrease glitch effect while rapid Margins change)
        private double delta;           // difference betwen current cursor position and position of the cursor at the last event
        private bool PrevState;         // previous state

        private double _height = 400;   // height of control
        private double _width = 1000;   // width of control
        private int shade_delta = 20;   // bias of bottom shade related to knob
        private double _old_position;   // previous pointer position X
        /**
         * _additional_width_r - additional width for move in right direction
         * _additional_width_l - additional width for move in left direction
         * _additional_width - sum of additional widths
         */
        private double _additional_width_r, _additional_width_l, _additional_width;
        private bool tmpState;          // temporar state for IsOn property, used to define state an the move of knob

        private DispatcherTimer timer;              // timer for knob's width decrease on the move
        private DispatcherTimer timer_long;         // timer for knob's width while state change
        private DispatcherTimer timer_back;         // timer for background mirror-like effect
        private DispatcherTimer timer_knob;         // timer for knob fill and dry effect

        private LinearGradientBrush brush_back;     // linear gradient brush for background
        private LinearGradientBrush br_knob;        // linear gradient brush for knob

        public delegate void MyEventHandler(object sender, EventArgs args);
        public event MyEventHandler OnStateChanged;
        public event MyEventHandler OnValueChanged;

        private bool _isBeingDragged;               // indicates if drag state is on/off

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background",
                                             typeof(Color), typeof(SwitchPlus), new PropertyMetadata(Colors.Orange));

        public new Windows.UI.Color Background          //defines color of background
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

        /// <summary>
        /// moves mirror-like effect from left to right constantly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Blink(object sender, object e)
        {
            brush_back.GradientStops[0].Offset += 0.05;
            brush_back.GradientStops[1].Offset += 0.05;
            brush_back.GradientStops[2].Offset += 0.05;

            if (brush_back.GradientStops[0].Offset>=2)
            {
                brush_back.GradientStops[0].Offset = 0;
                brush_back.GradientStops[1].Offset = 0.05;
                brush_back.GradientStops[2].Offset = 0.1;
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
        /// <summary>
        /// event handler which links knob.PointerPressed event with Fill- function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FillKnob(object sender, PointerRoutedEventArgs e)
        {
            br_knob.StartPoint = e.GetCurrentPoint(relativeTo: this.knob).Position;
            timer_knob.Tick += Fill_Tick;
            timer_knob.Start();
        }

        /// <summary>
        /// event handler which links knob.PointerPressed event with Dry- function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DryKnob(object sender, PointerRoutedEventArgs e)
        {
            br_knob.StartPoint = e.GetCurrentPoint(relativeTo: this.knob).Position;
            timer_knob.Tick += Dry_Tick;
            timer_knob.Start();
        }

        /// <summary>
        /// method describes 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Fill_Tick(object sender, object e)
        {
            if (br_knob.GradientStops[1].Offset <= 2)
            {
                br_knob.GradientStops[1].Offset += 0.1;
            }
            else
            {
                timer_knob.Stop();
                timer_knob.Tick -= Fill_Tick;
            }
        }

        private void Dry_Tick(object sender, object e)
        {
            if (br_knob.GradientStops[1].Offset - 0.1 > 0)
            {
                br_knob.GradientStops[1].Offset -= 0.1;
            }
            else
            {
                br_knob.GradientStops[1].Offset = 0;
                timer_knob.Stop();
                timer_knob.Tick -= Dry_Tick;
            }
        }

        public bool IsOn
        {
            get
            {
                return (bool)GetValue(IsOnProperty);
            }
            set
            {
                SetValue(IsOnProperty, value);
                if (value != PrevState)
                {
                    OnStateChanged(this, new EventArgs());
                }
                PrevState = value;              // because xaml compiler does not correctly support any operation betwen SetValue and begining of set block\
                                                // (compiler indicates error but still works ¯\_(ツ)_/¯  ), here is needed previous state identifier to rise state change event
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
        /// constructor (lol)
        /// </summary>
        public SwitchPlus()
        {
            this.InitializeComponent();
            this.Loaded += SwitchPlus_Loaded;
            knob_fill.PointerPressed += FillKnob;
            knob_fill.PointerReleased += DryKnob;
        }

        /// <summary>
        /// calls SwitchPlus values initialization after control Loaded - event;
        /// afterwards removes 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchPlus_Loaded(object sender, RoutedEventArgs e)
        {
            SwitchInitState();
            this.Loaded -= SwitchPlus_Loaded;
        }

        /// <summary>
        /// initializes all veraebles and bindings
        /// </summary>
        private void SwitchInitState()
        {
            main_shade.Height = _height;
            shade.Height = _height - KnobPadding * 2;
            knob.Height = _height - KnobPadding * 2;
            knob_fill.Height = _height - KnobPadding * 2;

            main_shade.Width = _height;
            shade.Width = _height - KnobPadding * 2;
            knob.Width = _height - KnobPadding * 2;
            knob_fill.Width = _height - KnobPadding * 2;

            main_shade.Opacity = 0.3;
            shade.Opacity = 0.5;

            if (IsOn)
            {
                main_shade.Margin = new Thickness(_width - _height, 0, 0, 0);
                shade.Margin = new Thickness(_width - _height + KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(_width - _height + KnobPadding, KnobPadding, 0, 0);
                knob_fill.Margin = new Thickness(_width - _height + KnobPadding, KnobPadding, 0, 0);
            }
            else
            {
                main_shade.Margin = new Thickness(0, 0, 0, 0);
                shade.Margin = new Thickness(KnobPadding, shade_delta + KnobPadding, 0, 0);
                knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                knob_fill.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
            }

            main_shade.Fill = new SolidColorBrush(Colors.Gray);
            shade.Fill = new SolidColorBrush(Colors.Gray);
            rectangle1.Fill = new SolidColorBrush(Background);

            brush_back = linearGradientBrush;
            br_knob = linearGradientBrush_k;
            br_knob.GradientStops[0].Color = Background;

            SetTimers();

            knob.PointerPressed += FillKnob;
        }

        /// <summary>
        /// Switches state with stretch effect 
        /// </summary>
        private void LongSwitchState()
        {
            if (IsOn)
            {
                _additional_width_r = _width - main_shade.Margin.Left - _height;
            }
            else
            {
                _additional_width_l = main_shade.Margin.Left;
                if (_additional_width_l == 0) _additional_width_l += _additional_width_r;
                _additional_width_r = 0;
            }
            timer_long.Start();
        }

        /// <summary>
        /// Event handler for Canvas 2 PointerPressed event. Initializes knob drag state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C2_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is Rectangle && !e.OriginalSource.Equals(C2.Children[0]))
            {
                _isBeingDragged = true;
                tmpState = IsOn;
            }
        }

        /// <summary>
        /// Event handler for Canvas 2 PointerPressed event. Turens off drag state 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C2_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isBeingDragged = false;
            delta = 0;
            _old_position = 0;
            position = 0;
            timer.Stop();

            if (!drag)
            {
                if (IsOn)
                    IsOn = false;
                else
                    IsOn = true;
            }
            else
                IsOn = tmpState;
            LongSwitchState();
            drag = false;
        }

        /// <summary>
        /// event handler for pointer move over whole controle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isBeingDragged)
            {
                if (!timer.IsEnabled)
                    timer.Start();
                drag = true;

                PointerPoint pp = e.GetCurrentPoint(relativeTo: this.C2);
                position = pp.Position.X;
                if (_old_position != 0)
                    delta = position - _old_position;
                else
                    delta = 0;
                if (OnValueChanged != null && delta != 0)
                    OnValueChanged(this, new EventArgs());
                if (position <= _width - _height && position >= _height / 2)
                {
                    if (delta < 0)
                    {
                        if (_additional_width_r != 0)
                        {
                            mod = true;
                            _additional_width_r += delta;
                            if (_additional_width_r < 0)
                            {
                                _additional_width_r = 0; mod = false;
                            }
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
                            mod = true;
                            _additional_width_l -= delta;
                            if (_additional_width_l < 0)
                            {
                                _additional_width_l = 0;
                                mod = false;
                            }
                        }
                        else
                        {
                            _additional_width_r += delta;
                        }
                    }
                }
                DrawKnob();
                double percentage = (main_shade.Margin.Left + _height/2) / _width; // checks if center of knob surpasses 1/2 of the control
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

        /// <summary>
        /// recounts knob's position
        /// makes knob stay in borders of control
        /// </summary>
        private void DrawKnob()
        {
            if ((delta > 0 &&_additional_width > position - _height) || (delta < 0 && _additional_width > _width - position - _height))
                mod = true;
            else
                mod = false;

            main_shade.Width = _height + _additional_width;
            shade.Width = _height + _additional_width - KnobPadding * 2;
            knob.Width = _height + _additional_width - KnobPadding * 2;
            knob_fill.Width = _height + _additional_width - KnobPadding * 2;


            if (delta > 0)
            {
                double res = position - _height / 2 - _additional_width;

                if (!mod)
                {
                    if (position >= _width - _height / 2)
                    {
                        main_shade.Margin = new Thickness(_width - _height - _additional_width, 0, 0, 0);
                        shade.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding + shade_delta, 0, 0);
                        knob.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                        knob_fill.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                    }
                    else
                    {
                        if (position <= _height / 2)
                        {
                            main_shade.Margin = new Thickness(0, 0, 0, 0);
                            shade.Margin = new Thickness(KnobPadding, KnobPadding + shade_delta, 0, 0);
                            knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                            knob_fill.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                        }
                        else
                        {
                            main_shade.Margin = new Thickness(res, 0, 0, 0);
                            shade.Margin = new Thickness(res + KnobPadding, KnobPadding + shade_delta, 0, 0);
                            knob.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                            knob_fill.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                        }
                    }
                }
            }
            else
            {
                double res = position - _height / 2;
                if (!mod)
                {
                    if (position <= _height / 2)
                    {
                        main_shade.Margin = new Thickness(0, 0, 0, 0);
                        shade.Margin = new Thickness(KnobPadding, KnobPadding + shade_delta, 0, 0);
                        knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                        knob_fill.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                    }
                    else
                    {
                        if (position >= _width - _height / 2)
                        {
                            main_shade.Margin = new Thickness(_width - _height - _additional_width, 0, 0, 0);
                            shade.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding + shade_delta, 0, 0);
                            knob.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                            knob_fill.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                        }
                        else
                        {
                            main_shade.Margin = new Thickness(res, 0, 0, 0);
                            shade.Margin = new Thickness(res + KnobPadding, KnobPadding + shade_delta, 0, 0);
                            knob.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                            knob_fill.Margin = new Thickness(res + KnobPadding, KnobPadding, 0, 0);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// initializes all timers in switch control
        /// </summary>
        private void SetTimers()
        {
            timer = new DispatcherTimer();       // initializes timer for stretching knob
            timer.Tick += RollBack;              // timer decreases width of knob with time on move
            timer.Interval = new TimeSpan(0, 0, 0, 0, 5);

            timer_long = new DispatcherTimer();  // initializes timer for stretching knob
            timer_long.Tick += RollIn;           // timer decreases width of knob on return to border
            timer_long.Interval = new TimeSpan(0, 0, 0, 0, 5);

            timer_back = new DispatcherTimer();  // initializes timer for mirror-like effect 
            timer_back.Tick += Blink;            // on background of knob
            timer_back.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer_back.Start();

            timer_knob = new DispatcherTimer();  // initializes timer for knob filling and drying effects on interaction
            timer_knob.Interval = new TimeSpan(0, 0, 0, 0, 20);
        }


        /// <summary>
        /// timer function which decreases width of knob on the move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RollBack(object sender, object e)
        {
            _additional_width_r -= 40;
            if (_additional_width_r <= 0)
            {
                _additional_width_r = 0;
                mod = false;
            }
            _additional_width_l -= 40;
            if (_additional_width_l <= 0)
            {
                _additional_width_l = 0;
                mod = false;
            }
            _additional_width = _additional_width_l + _additional_width_r;

            DrawKnob();

            if(!_isBeingDragged && _additional_width==0)
                timer.Stop();
        }

        /// <summary>
        /// timer function which rolls knob to border depending on switch state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RollIn(object sender, object e)
        {
            timer.Stop();
            _additional_width_r -= 40;
            if (_additional_width_r <= 0)
                _additional_width_r = 0;
            _additional_width_l -= 40;
            if (_additional_width_l <= 0)
                _additional_width_l = 0;
            double _additional_width = _additional_width_l + _additional_width_r;

            main_shade.Width = _height + _additional_width;
            shade.Width = _height + _additional_width - KnobPadding * 2;
            knob.Width = _height + _additional_width - KnobPadding * 2;
            knob_fill.Width = _height + _additional_width - KnobPadding * 2;

            if (IsOn)
            {
                main_shade.Margin = new Thickness(_width - _height - _additional_width, 0, 0, 0);
                shade.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding + shade_delta, 0, 0);
                knob.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
                knob_fill.Margin = new Thickness(_width - _height - _additional_width + KnobPadding, KnobPadding, 0, 0);
            }
            else
            {
                main_shade.Margin = new Thickness(0, 0, 0, 0);
                shade.Margin = new Thickness(KnobPadding, KnobPadding + shade_delta, 0, 0);
                knob.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
                knob_fill.Margin = new Thickness(KnobPadding, KnobPadding, 0, 0);
            }

            if (_additional_width == 0)
                timer_long.Stop();
        }
    }
}

/**
 * Somehow wrote and checked with God's/Devil's/Budda's/Ktulhu's etc. help. Anyway thanks.
 * 
 * Sign:
 * Bogdanenko Mykola/Niko, Nisefal : 12.04.2017
 */