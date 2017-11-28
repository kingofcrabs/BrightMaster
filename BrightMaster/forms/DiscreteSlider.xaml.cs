using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.ComponentModel;

namespace ExtControls
{
    public delegate void ValueChanged(object sender, int value);

    public partial class DiscreteSlider : UserControl, INotifyPropertyChanged
    {
        #region Dependency properties and their associated normal properties

        public static readonly DependencyProperty SliderBrushProperty = DependencyProperty.Register("SliderBrush", typeof(Brush), typeof(DiscreteSlider), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 100, 160, 220))));
        public static readonly DependencyProperty ThumbBrushProperty = DependencyProperty.Register("ThumbBrush", typeof(Brush), typeof(DiscreteSlider), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 160, 220))));
        public static readonly DependencyProperty ThumbEffectProperty = DependencyProperty.Register("ThumbEffect", typeof(Effect), typeof(DiscreteSlider), new PropertyMetadata(new DropShadowEffect { Color = Color.FromArgb(255, 21, 90, 116), ShadowDepth = 1 }));
        
        public Brush SliderBrush
        {
            get { return (Brush)GetValue(SliderBrushProperty); }
            set { SetValue(SliderBrushProperty, value); }
        }

        public Brush ThumbBrush
        {
            get { return (Brush)GetValue(ThumbBrushProperty); }
            set { SetValue(ThumbBrushProperty, value); }
        }

        public Effect ThumbEffect
        {
            get { return (Effect)GetValue(ThumbEffectProperty); }
            set { SetValue(ThumbEffectProperty, value); }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        int value;
        public int Value
        {
            get { return value; }
            private set
            {
                this.value = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        int[] values = new int[] { 5, 4, 3, 2, 1 };
        double[] yCoords = null;
        int selectedIndex = 0;

        public DiscreteSlider()
        {
            this.DataContext = this;
            InitializeComponent();
            this.Loaded += DiscreteSlider_Loaded;
            
        }

        void DiscreteSlider_Loaded(object sender, RoutedEventArgs e)
        {
            Reset(values);
        }

        Brush grayBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        double vGap = 0;
        public void Reset(int[] values)
        {
            if (values != null && values.Length > 1)
            {
                ticksCanvas.Children.Clear();
                this.values = values;
                yCoords = new double[values.Length];
                vGap = ticksCanvas.ActualHeight / (values.Length - 1);
                for (int i = 0; i < values.Length; i++)
                {
                    Line line = new Line();                    
                    line.Stroke = grayBrush;
                    line.StrokeThickness = 1;
                    line.X2 = 12;
                    line.Y1 = line.Y2 = Math.Round(i * vGap);
                    ticksCanvas.Children.Add(line);
                    yCoords[i] = line.Y1;
                }
                sliderCanvas.SetValue(Canvas.TopProperty, yCoords.First());
                Value = values.First();
                selectedIndex = 0;
            }
        }

        bool isMoving = false;
        double lastMouseY = 0;

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMoving = true;
            lastMouseY = e.GetPosition(sliderCanvas).Y;
            sliderThumb.CaptureMouse();
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMoving && values != null && values.Length > 1)
            {
                sliderCanvas.SetValue(Canvas.TopProperty, yCoords[selectedIndex]);
            }
            isMoving = false;
            sliderThumb.ReleaseMouseCapture();
        }

        private void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving && values != null && values.Length > 1)
            {
                double y = e.GetPosition(LayoutRoot).Y - lastMouseY;
                if (y >= yCoords.First() && y <= yCoords.Last())
                {
                    sliderCanvas.SetValue(Canvas.TopProperty, y);
                }
                for (int i = 0; i < yCoords.Length; i++)
                {
                    if (y > yCoords[i] - vGap / 2 && y < yCoords[i] + vGap / 2)
                    {
                        if (selectedIndex != i)
                        {
                            Value = values[i];
                            selectedIndex = i;
                        }
                        break;
                    }
                }
            }
        }
    }
}
