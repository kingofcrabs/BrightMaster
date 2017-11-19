using BrightMaster.data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace BrightMaster
{
    /// <summary>
    /// Interaction logic for XYCurves.xaml
    /// </summary>
    public partial class XYCurves : Window
    {
        private Brightness brightness;
        
        public PlotModel ModelX { get; set; }
        public PlotModel ModelY { get; set; }

        public XYCurves(Brightness brightness)
        {
            this.brightness = brightness;
            InitializeComponent();
            this.Loaded +=XYCurves_Loaded;
            myCanvas.MouseDown += myCanvas_MouseDown;
        }

        void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                return;

            object original = e.OriginalSource;

            if (!original.GetType().Equals(typeof(ScrollViewer)))
            {
                if (FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(original as DependencyObject) != null)
                {
                    return;
                }

            }

            var pt = e.GetPosition(myCanvas);
            myCanvas.OnCurveMouseDown(pt);
          

            int xPixel = (int)((pt.X / myCanvas.ActualWidth) * brightness.Width);
            int yPixel = (int)((pt.Y / myCanvas.ActualHeight) * brightness.Height);
            List<float> yVals = brightness.GetVerticalLineVals(xPixel);
            List<float> xVals = brightness.GetHorizontoalLineVals(yPixel);
            ModelX.Series.Clear();
            ModelX.Series.Add(CurveModel.CreateSeries(xVals));
            ModelY.Series.Clear();
            ModelY.Series.Add(CurveModel.CreateSeries(yVals,false));
            ModelX.InvalidatePlot(true);
            ModelY.InvalidatePlot(true);
        }

        private parentItem FindVisualParent<parentItem>(DependencyObject obj) where parentItem : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && !parent.GetType().Equals(typeof(parentItem)))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as parentItem;
        }
        void XYCurves_Loaded(object sender, RoutedEventArgs e)
        {
            List<float> xVals = brightness.GetHorizontoalLineVals(brightness.Height / 2);
            List<float> yVals = brightness.GetVerticalLineVals(brightness.Width / 2);
           
            myCanvas.OnCurveMouseDown(new Point(myCanvas.ActualWidth / 2, myCanvas.ActualHeight / 2));
            this.ModelX = CurveModel.CreateModel(xVals, true);
            this.ModelY = CurveModel.CreateModel(yVals, false);
            this.DataContext = this;

            var bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            myCanvas.SetBkGroundImage(bmpImage);
            ResetSlider(discreteSlider1, 8);
            discreteSlider1.PropertyChanged += discreteSlider1_PropertyChanged;
        }

        void UpdateImage(int grayLevelCnt)
        {
            var sparseGrayVals = brightness.GetSparseGrayLevels(grayLevelCnt);
            var bmpImage = ImageHelper.CreateImage(sparseGrayVals);
            myCanvas.SetBkGroundImage(bmpImage);
           
        }

        void discreteSlider1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //discreteSlider1.Value
            //this.IsEnabled = false;
            UpdateImage(discreteSlider1.Value);
            //this.IsEnabled = true;
        }

        private void ResetSlider(ExtControls.DiscreteSlider slider, int count)
        {
            int[] values = new int[count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (int)Math.Pow(2, i + 1);
            }
            slider.Reset(values.OrderByDescending(x => x).ToArray());
        }
    }
}
