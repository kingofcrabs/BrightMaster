using BrightMaster.data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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
            InitializeComponent();
            this.Loaded +=XYCurves_Loaded;
           
            List<float> xVals = brightness.GetHorizontoalLineVals(brightness.Height/2);
            List<float> yVals = brightness.GetVerticalLineVals(brightness.Width / 2);
            this.brightness = brightness;
            myCanvas.OnCurveMouseDown(new Point(myCanvas.ActualWidth / 2, myCanvas.ActualHeight / 2));
            this.ModelX = CurveModel.CreateModel(xVals, true);
            this.ModelY = CurveModel.CreateModel(yVals,false); 
            this.DataContext = this;
            myCanvas.MouseDown += myCanvas_MouseDown;
        }

        void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
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

    
        void XYCurves_Loaded(object sender, RoutedEventArgs e)
        {
            
            var bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            string sImgFile = "d:\\test.jpg";
            ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
            myCanvas.SetBkGroundImage(bmpImage);
        }

       
    }
}
